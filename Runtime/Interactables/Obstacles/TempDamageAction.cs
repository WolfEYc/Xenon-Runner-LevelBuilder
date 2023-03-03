using System;
using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunSim
{
	public class TempDamageAction : MonoBehaviour, IDamageable
	{
		[SerializeField] UnityEvent damaged, backToLife;
		[SerializeField] Timer timer;
		[SerializeField] float offTime;
		[SerializeField] GateOpener opener;
		
		void Awake()
		{
			timer.Expired += backToLife.Invoke;
		}

		void Start()
		{
			if (opener != null)
			{
				opener.Damageables.Add(this);
			}
		}

		public void Damage()
		{
			damaged.Invoke();
			timer.StartCountdown(offTime);
		}
	}
}