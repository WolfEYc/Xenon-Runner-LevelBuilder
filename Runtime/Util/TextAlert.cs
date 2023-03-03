using TMPro;
using UnityEngine;

namespace SpeedrunSim
{
    [RequireComponent(typeof(TMP_Text),typeof(Animator), typeof(Timer))]
    public class TextAlert : Singleton<TextAlert>
    {
        TMP_Text _text;
        Animator _animator;
        Timer _timer;
        static readonly int Alert1 = Animator.StringToHash("alert");
        static readonly int Hide = Animator.StringToHash("hide");


        protected override void Awake()
        {
            base.Awake();
            _text = GetComponent<TMP_Text>();
            _animator = GetComponent<Animator>();
            _timer = GetComponent<Timer>();
            _timer.Expired += Expired;
        }

        void Expired()
        {
            _animator.SetTrigger(Hide);
        }

        public void Alert(string text, float time = 1f)
        {
            _text.SetText(text);
            _animator.SetTrigger(Alert1);
            _timer.StartCountdown(time);
        }
        
        
    }
}
