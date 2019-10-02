using Common.Audio;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample.Windows
{
	public class Popup1 : Popup
	{
#pragma warning disable 649
		[SerializeField] private Button _playSoundButton;
		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		public const string Id = "popup_1";
		public override string WindowId => Id;

		protected override void Start()
		{
			base.Start();
			_playSoundButton.onClick.AddListener(() => _audioManager.PlayMusic("main_track"));
		}

		protected override void OnDestroy()
		{
			_playSoundButton.onClick.RemoveAllListeners();
			base.OnDestroy();
		}
	}
}