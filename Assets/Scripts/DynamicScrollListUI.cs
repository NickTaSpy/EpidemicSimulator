using Epsim.Profile.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Epsim
{
    public class DynamicScrollListUI : MonoBehaviour
    {
        public List<(Button Button, string Name)> Elements = new List<(Button Button, string Name)>();

        [SerializeField] private RectTransform Content;
        [SerializeField] private Button ElementTemplate;

        public void Add(string name, UnityAction callback)
        {
            var newButton = Instantiate(ElementTemplate, Content.transform);
            newButton.GetComponentInChildren<TMP_Text>().text = name;
            newButton.gameObject.SetActive(true);

#if UNITY_EDITOR
            newButton.name = name;
#endif

            newButton.onClick.AddListener(callback);
            Elements.Add((newButton, name));
        }

        public void Remove(Button button)
        {
            var index = Elements.FindIndex(x => x.Button == button);

            if (index >= 0)
            {
                Destroy(Elements[index].Button);
                Elements.RemoveAt(index);
            }
        }

        public void Remove(string name)
        {
            var index = Elements.FindIndex(x => x.Name == name);

            if (index >= 0)
            {
                Destroy(Elements[index].Button);
                Elements.RemoveAt(index);
            }
        }

        public void RemoveAll()
        {
            foreach (var element in Elements)
            {
                Destroy(element.Button);
            }

            Elements.Clear();
        }
    }
}