using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tools;
using Tools.Utils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using static Facade;

public class Game : GameSystem
{
	public const string GAME_MUSIC_VOLUME = "musicVolume";
	public const float MIN_MUSIC_VOLUME = -60f;

	public delegate void GameEventHandler();

	// Game States Events
	public event GameEventHandler OnStart;
	public event GameEventHandler OnGameOver;
	public event GameEventHandler OnPause;

	// Level States Events
	public event GameEventHandler OnPreparationPhase;
	public event GameEventHandler OnBuildingPhase;
	public event GameEventHandler OnTransitionPhase;
	public event GameEventHandler OnShootingPhase;
	public event GameEventHandler OnEndPhase;

	public static Game Instance { get; private set; }

	[Header("Level Parameters")]
	[SerializeField] private string levelName;
	[SerializeField] private int timer = 10;
	[SerializeField] private int bulletCount = 4;

	[Header("Audio")]
	[SerializeField] private AudioExpress gameMusic;

	[Header("Player")]
	[SerializeField] private PlayerController player;
	[SerializeField] private Transform spawnPlayer;

	[Header("Animations")]
	[SerializeField] private float fadDuration = 0.2f;

	[Header("References")]
	[SerializeField] private Dependency<FadScreen> _fader;
	[SerializeField] private Dependency<CinemachineImpulseSource> _impulse;
	[SerializeField] private Dependency<CinemachineVirtualCamera> _camera;
	[SerializeField] private Dependency<PostProcessVolume> _volume;
	[SerializeField] private AudioMixer mixer;
	[SerializeField] private Material transition;

	private GameState gameState;
	private LevelState levelState;
	private Coroutine loadingLevel;
	private float gameMusicVolume;
	private Coroutine inversingColor;
	private float startOrthographicSize;
	private FloatParameter startVignetteIntensity;
	private FloatParameter startChromaticAberation;
	private Tween zooming;
	private Tween updatingVignette;
	private Tween updatingChromatic;
	private Vignette vignette;
	private ChromaticAberration chromatic;

	public GameState GameState
	{
		get => gameState;
		set
		{
			gameState = value;
			switch (value)
			{
				case GameState.Play:
					OnStart?.Invoke();
					break;

				case GameState.GameOver:
					OnGameOver?.Invoke();
					break;

				case GameState.Pause:
					OnPause?.Invoke();
					break;
			}
		}
	}
	public LevelState LevelState
	{
		get => levelState;
		set
		{
			levelState = value;
			switch (value)
			{
				case LevelState.Preparation:
					OnPreparationPhase?.Invoke();
					break;
				case LevelState.Building:
					OnBuildingPhase?.Invoke();
					break;
				case LevelState.Transition:
					OnTransitionPhase?.Invoke();
					break;
				case LevelState.Shooting:
					OnShootingPhase?.Invoke();
					break;
				case LevelState.End:
					OnEndPhase?.Invoke();
					break;
			}
		}
	}
	public int Timer => timer;
	public string LevelName => levelName;
	public int BulletCount => bulletCount;
	public Transform Spawn => spawnPlayer;

	// Private Properties
	private FadScreen fader => _fader.Resolve(this);
	private CinemachineImpulseSource impulse => _impulse.Resolve(this);
	private CinemachineVirtualCamera currentCamera => _camera.Resolve(this);
	private PostProcessVolume volume => _volume.Resolve(this);

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	protected void Start()
	{
		// Post Processing
		transition.SetFloat("_isInversed", 0);
		volume.profile.TryGetSettings<Vignette>(out vignette);
		if (volume != null)
		{
			startVignetteIntensity = vignette.intensity;
		}

		volume.profile.TryGetSettings<ChromaticAberration>(out chromatic);
		if (volume != null)
		{
			startChromaticAberation = chromatic.intensity;
		}

		mixer.GetFloat(GAME_MUSIC_VOLUME, out gameMusicVolume);
		startOrthographicSize = currentCamera.m_Lens.OrthographicSize;

		fader.FadIn(fadDuration: fadDuration);
		gameMusic.Play();

		ResetPlayer();

		GameState = GameState.Play;
		LevelState = LevelState.Preparation;
	}

	protected override void Update()
	{
		base.Update();
	}

	private void LateUpdate()
	{
		if (LevelState == LevelState.Preparation && Input.GetButtonDown("Action"))
		{
			LevelState = LevelState.Building;
			ChangePhase();
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			ReloadLevel();
		}
		if (LevelState == LevelState.End && Input.GetButtonDown("Action"))
		{
			ReloadLevel();
		}
	}

	public void ResetPlayer()
	{
		player.transform.position = spawnPlayer.position;
	}

	private void ChangePhase()
	{
		InverseColor(0.1f);
		GenerateImpulse();
		Player.SetBreathingAnimation();
	}

	public void StartShootingPhase()
	{
		if (LevelState == LevelState.Building)
		{
			LevelState = LevelState.Transition;
			ChangePhase();
		}
	}

	public void GenerateImpulse()
	{
		impulse.GenerateImpulse();
	}

	public void InverseColor(float duration)
	{
		if (inversingColor != null)
		{
			StopCoroutine(inversingColor);
		}

		transition.SetFloat("_isInversed", 0);
		inversingColor = StartCoroutine(InversingColor(duration));
	}

	public void EndLevel()
	{
		LevelState = LevelState.End;
		ChangePhase();

		StartCoroutine(EndLevelCore());
	}

	private IEnumerator EndLevelCore()
	{
		List<BasicPlatform> platforms = FindObjectsOfType<BasicPlatform>().ToList();
		foreach (var platform in platforms)
		{
			platform.Disapear();
			yield return new WaitForSeconds(0.35f);
		}

		Player.transform.DOMove(Spawn.position, 1f).SetEase(Ease.OutCirc);

	}

	public void SetZoom(float value, float duration, Ease ease)
	{
		zooming?.Kill();

		currentCamera.LookAt = Player.transform;
		currentCamera.m_Lens.OrthographicSize = startOrthographicSize;

		DOTween.To(() => currentCamera.m_Lens.OrthographicSize, x => currentCamera.m_Lens.OrthographicSize = x, value, duration * 0.1f).SetEase(ease).OnComplete(() => ZoomComeback(duration, ease)).SetUpdate(true);
	}

	private void ZoomComeback(float duration, Ease ease)
	{
		DOTween.To(() => currentCamera.m_Lens.OrthographicSize, x => currentCamera.m_Lens.OrthographicSize = x, startOrthographicSize, duration).SetEase(ease).OnComplete(() => ResetCamera()).SetUpdate(true);
	}

	private void ResetCamera()
	{
		currentCamera.LookAt = null;
		currentCamera.transform.DORotate(Vector3.zero, 0.5f);
	}

	public void SetVignette(float value, float duration, Ease ease)
	{
		if (vignette == null)
		{
			Debug.LogWarning("Vignette effect has not been initialized. is PostProcessVolume component missing?");
			return;
		}

		updatingVignette?.Kill();
		vignette.intensity = startVignetteIntensity;
		updatingVignette = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, value, duration).SetEase(ease).SetLoops(2, LoopType.Yoyo);
	}

	public void SetChromaticAberation(float value, float duration, Ease ease)
	{
		if (chromatic == null)
		{
			Debug.LogWarning("Chromatic effect has not been initialized. is PostProcessVolume component missing?");
			return;
		}

		updatingChromatic?.Kill();
		chromatic.intensity = startChromaticAberation;
		updatingChromatic = DOTween.To(() => chromatic.intensity.value, x => chromatic.intensity.value = x, value, duration).SetEase(ease).SetLoops(2, LoopType.Yoyo);
	}

	private IEnumerator InversingColor(float duration)
	{
		transition.SetFloat("_isInversed", 1);
		yield return new WaitForSeconds(duration);
		transition.SetFloat("_isInversed", 0);
	}

	public void UpdateGameMusicVolume(float percentage)
	{
		gameMusicVolume = Mathf.Lerp(MIN_MUSIC_VOLUME, 0f, percentage);
		mixer.SetFloat(GAME_MUSIC_VOLUME, gameMusicVolume);
	}

	public void ReloadLevel()
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.ReloadLevel();
			}));
		}
	}

	public void LoadNextLevel()
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.LoadNextLevel();
			}));
		}
	}

	public void LoadMenu()
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.LoadLevelByName(Constants.MENU_SCENE);
			}));
		}
	}

	public void LoadSceneByName(string sceneName)
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.LoadLevelByName(sceneName);
			}));
		}
	}

	public void LoadSceneTransition(LevelLoading levelLoading)
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.OnLoadLevel(levelLoading);
			}));
		}
	}

	public void QuitGame()
	{
		if (loadingLevel == null)
		{
			loadingLevel = StartCoroutine(LoadLevelCore(

			content: () =>
			{
				LevelLoader.QuitGame();
			}));
		}
	}

	private IEnumerator LoadLevelCore(Action content = null)
	{
		if (inversingColor != null)
		{
			StopCoroutine(inversingColor);
		}
		Time.timeScale = 1f;

		yield return fader.FadOutCore(fadDuration: fadDuration);
		content?.Invoke();
	}
}