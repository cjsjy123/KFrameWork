using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace KFrameWork
{
	public static class ButtonWrap 
	{
		public static void AddListener<T> (this Button btn,Action<T> callback,T custom) 
		{
			btn.onClick.AddListener(delegate() {

				callback(custom);
			});
		}
		#region often Type
		public static void AddListener (this Button btn,Action<Button> callback) 
		{
			btn.onClick.AddListener(delegate() {
				
				callback(btn);
			});
		}

		public static void AddListener (this Button btn,Action<GameObject> callback) 
		{
			btn.onClick.AddListener(delegate() {
				
				callback(btn.gameObject);
			});
		}

		#endregion

		public static void AddListener (this Button btn,Action callback) 
		{
			btn.onClick.AddListener(delegate() {
				
				callback();
			});
		}

		public static void RemoveAllListner(this Button btn)
		{
			btn.onClick.RemoveAllListeners();
		}
    }
}


