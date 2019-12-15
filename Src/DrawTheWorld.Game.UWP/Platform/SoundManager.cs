using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawTheWorld.Core.Platform;
using DrawTheWorld.Core.UserData;
using FLib;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace DrawTheWorld.Game.Platform
{
	/// <summary>
	/// Plays background music and provides ability to play sounds.
	/// </summary>
	sealed class SoundManager
		: ISoundManager
	{
		private static readonly MetroLog.ILogger Logger = LogManager.GetLogger("Game.SoundManager");

		private static readonly Random Rnd = new Random();

		private readonly Dictionary<Sound, MediaElement> Sounds = new Dictionary<Sound, MediaElement>();
		private readonly MusicPlayer Player = null;
		private readonly UserStatistics Statistics = null;
		private readonly ISettingsProvider Settings = null;

		/// <summary>
		/// Initializes the object.
		/// </summary>
		/// <param name="userStatistics"></param>
		/// <param name="settings"></param>
		public SoundManager(UserStatistics userStatistics, ISettingsProvider settings)
		{
			Validate.Debug(() => userStatistics, v => v.NotNull());
			Validate.Debug(() => settings, v => v.NotNull());

			this.Statistics = userStatistics;
			this.Statistics.StatsAdded += this.OnStatisticsAdded;

			this.Settings = settings;

			this.Player = new MusicPlayer(this.Settings);
		}

		/// <inheritdoc />
		public void PlaySound(Sound sound)
		{
			MediaElement media = null;
			if (this.Sounds.TryGetValue(sound, out media))
			{
				if (media.CurrentState != MediaElementState.Playing)
				{
					Logger.Debug("Playing sound '{0}'.", sound);
					media.Volume = this.Settings.SoundsVolume;
					media.Play();
				}
			}
		}

		/// <summary>
		/// Initializes the manager(loads all necessary sound files in the background).
		/// </summary>
		/// <param name="container"></param>
		public async Task Initialize(Panel container)
		{
			IReadOnlyList<StorageFile> sounds = null, music = null;
			try
			{
				var installFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
				sounds = await (await installFolder.GetFolderAsync("Sounds")).GetFilesAsync();
				music = await (await installFolder.GetFolderAsync("Music")).GetFilesAsync();
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot list assets.", ex);
				return;
			}

			foreach (var snd in sounds)
				await this.CreateSound(snd, container);

			foreach (var ms in music)
				await this.CreateMusic(ms, container);

			this.Player.StartLoop();
		}

		private void OnStatisticsAdded(PackStatistics pack, BoardStatistics board)
		{
			var track = this.Player.Tracks.FirstOrDefault(t => t.PackId == pack.Id);
			if (track != null && !track.IsAvailable)
			{
				track.IsAvailable = TestForFinish(pack);
				if (track.IsAvailable)
					this.Player.FadeOutAndPlay(track);
			}
		}

		private async Task CreateSound(StorageFile snd, Panel container)
		{
			var name = Path.GetFileNameWithoutExtension(snd.Name);
			Logger.Debug("Loading sound '{0}'.", snd.Name);
			try
			{
				this.Sounds.Add(
					(Sound)Enum.Parse(typeof(Sound), name),
					await this.CreateMedia(snd, true, container)
					);
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot load sound from file '{0}'.".FormatWith(snd.Name), ex);
			}
		}

		private async Task CreateMusic(StorageFile ms, Panel container)
		{
			Logger.Debug("Loading music from '{0}'.", ms.Name);
			Track track = null;
			try
			{
				track = new Track { Media = await this.CreateMedia(ms, false, container) };
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot load music from '{0}'.".FormatWith(ms.Name), ex);
				return;
			}

			var guidEnd = ms.Name.LastIndexOf(']');
			var guidStart = guidEnd != -1 ? ms.Name.LastIndexOf('[', guidEnd - 1) + 1 : -1;
			Guid id;

			if (guidStart != 0 && guidEnd != -1 && Guid.TryParse(ms.Name.Substring(guidStart, guidEnd - guidStart), out id))
			{
				track.PackId = id;
				var pack = this.Statistics[id];
				track.IsAvailable = pack != null && TestForFinish(pack);
			}
			else
				track.IsAvailable = true;

			var trackProps = await ms.Properties.GetMusicPropertiesAsync();
			track.Name =
				!string.IsNullOrWhiteSpace(trackProps.Artist) && !string.IsNullOrWhiteSpace(trackProps.Title) ?
				trackProps.Artist + " - " + trackProps.Title
				: ms.Name;

			this.Player.Add(track);
		}

		private async Task<MediaElement> CreateMedia(StorageFile source, bool isSound, Panel container)
		{
			var media = new MediaElement();
			container.Children.Add(media);
			media.IsLooping = false;
			media.AutoPlay = false;
			var content = await source.OpenReadAsync();
			media.SetSource(content, content.ContentType);
			media.AudioCategory = isSound ? AudioCategory.GameEffects : AudioCategory.GameMedia;
			return media;
		}

		private static bool TestForFinish(PackStatistics pack)
		{
			return pack.Boards.Values.All(b => b.Any(s => s.Result == Core.FinishReason.Correct));
		}

		sealed class MusicPlayer
		{
			private static readonly Duration FadeOutDuration = new Duration(TimeSpan.FromSeconds(1));

			private readonly Storyboard FadeOutTrack = new Storyboard();
			private readonly List<Track> _Tracks = new List<Track>();
			private readonly ISettingsProvider Settings = null;

			private Track Old = null;
			private Track Current = null;

			public IReadOnlyList<Track> Tracks
			{
				get { return this._Tracks; }
			}

			/// <summary>
			/// Initializes the player.
			/// </summary>
			public MusicPlayer(ISettingsProvider settings)
			{
				this.Settings = settings;
				this.Settings.PropertyChanged += this.OnSettingsChanged;

				var anim = new DoubleAnimation
				{
					Duration = FadeOutDuration,
					To = 0,
					EnableDependentAnimation = true
				};
				Storyboard.SetTargetProperty(anim, "Volume");
				this.FadeOutTrack.Children.Add(anim);
				this.FadeOutTrack.Completed += (s, e) => this.PlayTrack(this.Current);
			}

			/// <summary>
			/// Adds track to the list of available tracks.
			/// </summary>
			/// <param name="track"></param>
			public void Add(Track track)
			{
				Validate.Debug(() => track, v => v.NotNull());

				track.Media.MediaEnded += this.PlayNextTrack;
				this._Tracks.Add(track);
			}

			/// <summary>
			/// Fades out currently playing track and plays specified one.
			/// </summary>
			/// <param name="track"></param>
			public void FadeOutAndPlay(Track track)
			{
				Validate.Debug(() => track, v => v.NotNull().IsIn(this._Tracks).That(t => t.IsAvailable, v2 => v2.True()));

				if (this.Current != null && this.Current.Media.CurrentState == MediaElementState.Playing)
				{
					this.FadeOutTrack.Stop();
					Storyboard.SetTarget(this.FadeOutTrack.Children[0], this.Current.Media);

					this.Old = this.Current;
					this.Current = track;
					this.FadeOutTrack.Begin();
				}
				else
					this.PlayTrack(track);
			}

			/// <summary>
			/// Stops current music and plays specified track.
			/// </summary>
			/// <param name="track"></param>
			private void PlayTrack(Track track)
			{
				Validate.Debug(() => track, v => v.NotNull().IsIn(this._Tracks).That(t => t.IsAvailable, v2 => v2.True()));

				if (this.Current != null && this.Current.Media.CurrentState == MediaElementState.Playing)
					this.Current.Media.Stop();

				track.Media.Volume = this.Settings.MusicVolume;
				if (track.Media.CurrentState == MediaElementState.Opening)
					track.Media.MediaOpened += this.DelayedPlay;
				else
					track.Media.Play();
				this.Old = this.Current;
				this.Current = track;
			}

			/// <summary>
			/// Starts music loop.
			/// </summary>
			public void StartLoop()
			{
				this.PlayNextTrack(null, null);
			}

			private void DelayedPlay(object sender, Windows.UI.Xaml.RoutedEventArgs e)
			{
				var media = (MediaElement)sender;
				media.MediaOpened -= this.DelayedPlay;
				media.Play();
			}

			private void PlayNextTrack(object sender, RoutedEventArgs e)
			{
				if (this._Tracks.Count > 0 && this._Tracks.Any(m => m.IsAvailable))
				{
					int idx = 0;
					do
					{
						idx = Rnd.Next(0, this._Tracks.Count);
					} while (this._Tracks[idx].Media == sender || !this._Tracks[idx].IsAvailable);

					Logger.Debug("Playing '{0}'.", this._Tracks[idx].Name);
					this.PlayTrack(this._Tracks[idx]);
				}
			}

			private void OnSettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == this.Settings.NameOf(_ => _.MusicVolume) && this.Current != null)
					this.Current.Media.Volume = this.Settings.MusicVolume;
			}
		}

		private sealed class Track
		{
			public Guid? PackId = null;
			public bool IsAvailable = false;
			public string Name = null;
			public MediaElement Media = null;
		}
	}
}
