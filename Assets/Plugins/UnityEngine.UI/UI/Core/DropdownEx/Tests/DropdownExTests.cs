using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace UnityEngine.UI.Tests
{
    public class MultiSelectTests
    {
        public DropdownEx Dropdown = null;

        public GameObject Option1;
        public GameObject Option2;
        public GameObject Option3;

        public Toggle Toggle1;
        public Toggle Toggle2;
        public Toggle Toggle3;

        public IEnumerator Show()
        {
            Dropdown.Show();
            yield return WaitForObjectAppeared("Canvas/DropdownEx/Dropdown List");

            //
            // Collect our dropdown items we need for the test.
            //
            string path = "Dropdown List/Viewport/Content/Item 0: Option 1";
            Option1 = GameObject.Find(path);
            Assert.IsNotNull(Option1, "Expected to find gameobject: " + path);

            Toggle1 = Option1.GetComponent<Toggle>();
            Assert.IsNotNull(Toggle1, "Expected to find toggle component");

            path = "Dropdown List/Viewport/Content/Item 1: Option 2";
            Option2 = GameObject.Find(path);
            Assert.IsNotNull(Option2, "Expected to find gameobject: " + path);

            Toggle2 = Option2.GetComponent<Toggle>();
            Assert.IsNotNull(Toggle2, "Expected to find toggle component");

            path = "Dropdown List/Viewport/Content/Item 2: Option 3";
            Option3 = GameObject.Find(path);
            Assert.IsNotNull(Option3, "Expected to find gameobject: " + path);

            Toggle3 = Option3.GetComponent<Toggle>();
            Assert.IsNotNull(Toggle3, "Expected to find toggle component");
        }

        public IEnumerator LoadTestScene(float timeout = 30f)
        {
            bool sceneLoaded = false;
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                sceneLoaded = true;
            };
            SceneManager.LoadScene("DropdownEx Test Scene");

            // Wait for the scene to load
            while (!sceneLoaded && timeout > 0)
            {
                yield return new WaitForSeconds(.1f);
                timeout -= .1f;
            }

            Assert.IsTrue(timeout > 0, "Timeout waiting for scene to load");
            Assert.IsTrue(sceneLoaded, "Expected SceneManager sceneLoaded callback");
        }

        public IEnumerator ClickOption(GameObject go)
        {
            yield return Show();
            
            ExecuteEvents.Execute(go, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            yield return WaitForObjectDisappeared("Canvas/DropdownEx/Dropdown List");

            Option1 = null;
            Option2 = null;
            Option3 = null;
            Toggle1 = null;
            Toggle2 = null;
            Toggle3 = null;
        }

        public IEnumerator WaitForObjectDisappeared(string path)
        {
            var timeout = 5f;
            while (GameObject.Find(path) != null || timeout <= 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
        }

        public IEnumerator WaitForObjectAppeared(string path)
        {
            var timeout = 5f;
            while (null == GameObject.Find(path) || timeout <= 0)
            {
                yield return new WaitForSeconds(0.1f);
                timeout -= 0.1f;
            }
        }


        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CheckDefaults()
        {
            yield return LoadTestScene();

            Dropdown = GameObject.FindObjectOfType<DropdownEx>();

            Assert.IsNotNull(Dropdown, "Expected to find DropdownEx instance");
            Assert.IsTrue(Dropdown.options.Count == 3, "Expected 3 options after setup");
            Assert.IsTrue(Dropdown.SelectedCount == 1, "Expected one option selected after setup");
            Assert.IsFalse(Dropdown.AllowMultiSelect, "Expected multiple selections off by default");

            var count = 0;
            foreach (var opt in Dropdown.SelectedOptions)
                count++;

            Assert.IsTrue(count == 1, "Expected one option selected by default");
            Assert.IsTrue(Dropdown.value == 0, "Expected value to be zero by default");

            for (int i = 0; i < Dropdown.options.Count; i++)
            {
                Assert.AreEqual("Option " + (i + 1).ToString(), Dropdown.options[i].text, "Unexpected option text: " + Dropdown.options[i].text);

                if (0 == i)
                {
                    Assert.IsTrue(Dropdown.options[i].selected, "Expected first option selected");
                }
                else
                {
                    Assert.IsFalse(Dropdown.options[i].selected,
                        string.Format("Expected all but first option deselected by default but option index {0} was selected", i));
                }
            }
        }

        [UnityTest]
        public IEnumerator EnableMultiselect()
        {
            yield return CheckDefaults();

            var deselected = uint.MaxValue;
            Dropdown.onItemDeselected.AddListener((i) =>
            {
                deselected = i;
            });

            var selected = uint.MaxValue;
            Dropdown.onItemSelected.AddListener((i) =>
            {
                selected = i;
            });

            var oldValue = Dropdown.value;
            Assert.AreEqual(0, oldValue, "Expected old value to be zero");

            var newValue = uint.MaxValue;
            Dropdown.onValueChanged.AddListener((i) =>
            {
                newValue = i;
            });

            Dropdown.AllowMultiSelect = true;
            yield return new WaitForEndOfFrame();

            // check that the proper events were fired
            Assert.AreEqual(uint.MaxValue, deselected, "Expected NOT to fire onItemDeselected");

            // By default the first item is selected. When we switch to multiselect,
            // onItemSelected will fire with a 1 (0 == no items selected, 1 is the first item - index 0)
            Assert.AreEqual(uint.MaxValue, selected, "Didn't expect onItemSelected to be called");
            Assert.AreEqual(uint.MaxValue, newValue, "Didn't expect onValueChanged to be called");

            Assert.IsTrue(0 == Dropdown.value, "Expected value to change to 1");
            Assert.IsTrue(0 == Dropdown.SelectedCount);

            var count = 0;
            foreach (var opt in Dropdown.SelectedOptions)
                count++;

            Assert.IsTrue(0 == count, "Expected SelectedOptions to return one option. It returned " + count.ToString());
            Assert.AreEqual(Dropdown.NothingSelectedText, Dropdown.captionText.text, "Expected caption to be " + Dropdown.NothingSelectedText);
        }

        [UnityTest]
        public IEnumerator DisableMultiselect()
        {
            yield return EnableMultiselect();

            var deselected = uint.MaxValue;
            Dropdown.onItemDeselected.AddListener((i) =>
            {
                deselected = i;
            });

            var selected = uint.MaxValue;
            Dropdown.onItemSelected.AddListener((i) =>
            {
                selected = i;
            });

            var oldValue = Dropdown.value;
            Assert.AreEqual(0, oldValue, "Expected old value to be 1");

            var newValue = uint.MaxValue;
            Dropdown.onValueChanged.AddListener((i) =>
            {
                newValue = i;
            });

            Dropdown.AllowMultiSelect = false;
            yield return new WaitForEndOfFrame();

            // check that the proper events were fired
            Assert.AreEqual(uint.MaxValue, deselected, "Expected NOT to fire onItemDeselected");
            Assert.AreEqual(uint.MaxValue, selected, "Expected onItemSelected NOT to fire");
            Assert.AreEqual(uint.MaxValue, newValue, "Expected onValueChanged NOT to fire");

            Assert.IsTrue(0 == Dropdown.value, "Expected value to remain zero");
            Assert.IsTrue(1 == Dropdown.SelectedCount);

            var count = 0;
            foreach (var opt in Dropdown.SelectedOptions)
                count++;

            Assert.IsTrue(1 == count, "Expected SelectedOptions to return one option. It returned " + count.ToString());
            Assert.AreEqual("Option 1", Dropdown.captionText.text, "Expected caption to be 'Option 1'");
        }

        [UnityTest]
        public IEnumerator SelectMultiple()
        {
            // When you enable multiselect, there will be nothing initially selected
            // (value == 0)
            yield return EnableMultiselect();

            Assert.IsFalse(Dropdown.options[0].selected, "Expected first item to NOT be selected");
            Assert.IsTrue(0 == Dropdown.SelectedCount, "Expected zero items selected");

            //
            // In this test we will click options 1 & 2 and verify they are
            // both toggled on
            //
            yield return Show();
            yield return ClickOption(Option1);

            //
            // Verify option 1 is toggled on
            //
            yield return Show();
            Assert.IsNotNull(Toggle1, "Expected to still have toggle1 instance");
            Assert.IsTrue(Toggle1.isOn, "Expected option 1 toggle to be on");

            yield return ClickOption(Option2);

            yield return Show();
            Assert.IsNotNull(Toggle1, "Expected to still have toggle1 instance");
            Assert.IsNotNull(Toggle2, "Expected to still have toggle2 instance");
            Assert.IsNotNull(Toggle3, "Expected to still have toggle3 instance");

            Assert.IsTrue(Toggle1.isOn, "Expected option 1 toggle to be on");
            Assert.IsTrue(Toggle2.isOn, "Expected option 2 toggle to switch to on");
            Assert.IsFalse(Toggle3.isOn, "Expected toggle3 to remain off");

            Assert.IsTrue(2 == Dropdown.SelectedCount,
                string.Format("Expected two options selected but {0} were", Dropdown.SelectedCount));

            var items = new List<DropdownEx.OptionData>();
            foreach (var opt in Dropdown.SelectedOptions)
                items.Add(opt);

            Assert.IsTrue(2 == items.Count, "Expected SelectedOptions to return 2 items");
            Assert.IsTrue(items[0].selected, "Expected item 1 to be selected");
            Assert.IsTrue(items[1].selected, "Expected item 2 to be selected");

            Assert.IsTrue(Dropdown.options[0].selected, "Expected option 1 selected property to be true");
            Assert.IsTrue(Dropdown.options[1].selected, "Expected option 2 selected property to be true");
            Assert.IsFalse(Dropdown.options[2].selected, "Expected Option 3 selected property to be false");
        }

        [UnityTest]
        public IEnumerator DeselectReselect()
        {
            yield return SelectMultiple();  // selects options 1 & 2

            yield return Show();

            //
            // We expect option 1 and 2 to be on
            //
            Assert.IsNotNull(Toggle1, "Could not find Toggle component on option 1");
            Assert.IsTrue(Toggle1.isOn, "Expected option 1 Toggle component to be on");

            Assert.IsNotNull(Toggle2, "Could not find Toggle component on option 2");
            Assert.IsTrue(Toggle2.isOn, "Expected option 2 Toggle component to be on");

            Assert.IsNotNull(Toggle3, "Could not find Toggle component on option 3");
            Assert.IsFalse(Toggle3.isOn, "Expected option 3 Toggle component to be off");

            //
            // Click option 2 off
            //
            yield return ClickOption(Option2);
            yield return Show();

            //
            // Click option 3 on
            //
            yield return ClickOption(Option3);
            yield return Show();

            //
            // Verify final state.  Options 1 and 3 should be selected
            //
            Assert.IsNotNull(Toggle1, "Expected to still have toggle1 instance");
            Assert.IsNotNull(Toggle2, "Expected to still have toggle2 instance");
            Assert.IsNotNull(Toggle3, "Expected to still have toggle3 instance");

            Assert.IsTrue(Toggle1.isOn, "Expected option 1 toggle to remain on");
            Assert.IsFalse(Toggle2.isOn, "Expected option 2 toggle to switch to off");
            Assert.IsTrue(Toggle3.isOn, "Expected toggle3 to switch to on");

            Assert.IsTrue(2 == Dropdown.SelectedCount,
                string.Format("Expected two options selected but {0} were", Dropdown.SelectedCount));

            Assert.IsTrue(Dropdown.options[0].selected, "Expected option 1 selected property to be true");
            Assert.IsFalse(Dropdown.options[1].selected, "Expected option 2 selected property to be false");
            Assert.IsTrue(Dropdown.options[2].selected, "Expected Option 3 selected property to be true");

            var items = new List<DropdownEx.OptionData>();
            foreach (var opt in Dropdown.SelectedOptions)
                items.Add(opt);

            Assert.IsTrue(2 == items.Count, "Expected SelectedOptions to return 2 items");
        }

        [UnityTest]
        public IEnumerator Events()
        {
            yield return EnableMultiselect(); // initially no options selected

            yield return Show();

            var deselected = uint.MaxValue;
            Dropdown.onItemDeselected.AddListener((i) =>
            {
                deselected = i;
            });

            var selected = uint.MaxValue;
            Dropdown.onItemSelected.AddListener((i) =>
            {
                selected = i;
            });

            var oldValue = Dropdown.value;
            Assert.AreEqual(0, oldValue, "Expected old value to be 0");
            Assert.IsTrue(Dropdown.SelectedCount == 0, "Expected nothing to be selected");

            var newValue = uint.MaxValue;
            Dropdown.onValueChanged.AddListener((i) =>
            {
                newValue = i;
            });


            //
            // Click option 2 on
            //
            yield return ClickOption(Option2);

            Assert.AreEqual(uint.MaxValue, deselected, "Did not expect onItemDeselected to be called");
            Assert.AreEqual(1, selected, "Expected onItemSelected with index 1");
            Assert.AreEqual(2, newValue, "Expected onValueChanged to be called with value == 3");
            Assert.AreEqual(2, Dropdown.value, "Expected value to be 3 (options 1 & 2");

            yield return Show();

            // reset our event state variables
            deselected = uint.MaxValue;
            selected = uint.MaxValue;
            oldValue = Dropdown.value;
            newValue = uint.MaxValue;

            //
            // Click option 3 on
            //
            yield return ClickOption(Option3);

            Assert.AreEqual(uint.MaxValue, deselected, "Did not expect onItemDeselected to be called");
            Assert.AreEqual(2, selected, "Expected onItemSelected with index 1");
            Assert.AreEqual(6, newValue, "Expected onValueChanged to be called with value == 3");
            Assert.AreEqual(6, Dropdown.value, "Expected value to be 3 (options 1 & 2");

            yield return Show();

            deselected = uint.MaxValue;
            selected = uint.MaxValue;
            oldValue = Dropdown.value;
            newValue = uint.MaxValue;

            //
            // Click option 2 to turn it off
            //
            yield return ClickOption(Option2);

            Assert.AreEqual(1, deselected, "Did not expect onItemDeselected to be called");
            Assert.AreEqual(uint.MaxValue, selected, "Expected onItemSelected with index 1");
            Assert.AreEqual(4, newValue, "Expected onValueChanged to be called with value == 3");
            Assert.AreEqual(4, Dropdown.value, "Expected value to be 3 (options 1 & 2");

        }

        [UnityTest]
        public IEnumerator SetValue()
        {
            yield return EnableMultiselect();  // no values selected to start

            var oldValue = Dropdown.value;
            Assert.IsTrue(0 == oldValue, "Expected current value to be 0");

            //
            // Setting the value to 3 selects option 1 & 2
            //
            Dropdown.value = 3;
            Assert.IsTrue(3 == Dropdown.value, "Expected new value to be 3");
            // when the value is set, Hide() is called internally but it
            // is a delayed destruction of the game objects. (about .15 seconds)
            yield return new WaitForSeconds(.5f);  


            Assert.AreEqual(2, Dropdown.SelectedCount, "Expected SelectedCount to be 2");

            Assert.IsTrue(Dropdown.options[0].selected, "Expected options[0].selected to be true");
            Assert.IsTrue(Dropdown.options[1].selected, "Expected options[1].selected to be true");
            Assert.IsFalse(Dropdown.options[2].selected, "Expected options[2].selected to be false");

            yield return Show();

            //
            // Collect our dropdown items we need for the test.
            //
            Assert.IsNotNull(Option1, "Expected to find Option1");
            Assert.IsNotNull(Toggle1, "Expected to find toggle component");
            Assert.IsTrue(Toggle1.isOn,
                string.Format("Expected toggle1 {0} to be on but it was {1}",
                    Toggle1.name, Toggle1.isOn));

            Assert.IsNotNull(Option2, "Expected to find Option2");
            Assert.IsNotNull(Toggle2, "Expected to find toggle component");
            Assert.IsTrue(Toggle2.isOn, "Expected toggle2 to be on");

            Assert.IsNotNull(Option3, "Expected to find Option3");
            Assert.IsNotNull(Toggle3, "Expected to find toggle component");
            Assert.IsFalse(Toggle3.isOn, "Expected toggle3 to be off");

            Assert.IsTrue(Dropdown.options[0].selected, "Expected options[0].selected to be true");
            Assert.IsTrue(Dropdown.options[1].selected, "Expected options[1].selected to be true");
            Assert.IsFalse(Dropdown.options[2].selected, "Expected options[2].selected to be true");
        }

        [UnityTest]
        public IEnumerator ClearValue()
        {
            // set up with options 1 and 2 selected
            yield return SetValue();
            Assert.IsTrue(Dropdown.value == 3, "Expected value to start at 3");
            
            Dropdown.value = 4;

            // when the value is set, Hide() is called internally but it
            // is a delayed destruction of the game objects. (about .15 seconds)
            yield return new WaitForSeconds(.5f);  

            yield return Show();

            //
            // Collect our dropdown items we need for the test.
            //
            Assert.IsNotNull(Option1, "Expected to find Option1");
            Assert.IsNotNull(Toggle1, "Expected to find toggle component");
            Assert.IsFalse(Toggle1.isOn, "Expected toggle1 to be off");

            Assert.IsNotNull(Option2, "Expected to find Option2");
            Assert.IsNotNull(Toggle2, "Expected to find toggle component");
            Assert.IsFalse(Toggle2.isOn, "Expected toggle2 to be off");

            Assert.IsNotNull(Option3, "Expected to find Option3");
            Assert.IsNotNull(Toggle3, "Expected to find toggle component");
            Assert.IsTrue(Toggle3.isOn, "Expected toggle3 to be on");
        }


    }

    public class SingleSelectTests
    {
        [UnityTest]
        public IEnumerator SelectItem()
        {
            var mst = new MultiSelectTests();
            yield return mst.CheckDefaults();

            yield return mst.Show();

            Assert.IsNotNull(mst.Option1, "Expected to find Option1");
            Assert.IsNotNull(mst.Toggle1, "Expected to find toggle component");
            Assert.IsTrue(mst.Toggle1.isOn,
                string.Format("Expected toggle1 {0} to be on but it was {1}",
                    mst.Toggle1.name, mst.Toggle1.isOn));

            Assert.IsNotNull(mst.Option2, "Expected to find Option2");
            Assert.IsNotNull(mst.Toggle2, "Expected to find toggle component");
            Assert.IsFalse(mst.Toggle2.isOn, "Expected toggle2 to be off");

            Assert.AreEqual(0, mst.Dropdown.value);
            Assert.IsTrue(1 == mst.Dropdown.SelectedCount);
            Assert.IsTrue(mst.Dropdown.options[0].selected, "Expected options[0].selected to be true");
            Assert.IsFalse(mst.Dropdown.options[1].selected, "Expected options[1].selected to be false");
            Assert.IsFalse(mst.Dropdown.options[2].selected, "Expected options[2].selected to be false");

            Assert.IsNotNull(mst.Option3, "Expected to find Option3");
            Assert.IsNotNull(mst.Toggle3, "Expected to find toggle component");
            Assert.IsFalse(mst.Toggle3.isOn, "Expected toggle3 to be off");

            //
            // Click option 3 to turn it on, turning option 1 off
            //
            yield return mst.ClickOption(mst.Option3);

            yield return mst.Show();

            Assert.AreEqual(2, mst.Dropdown.value);
            Assert.IsTrue(1 == mst.Dropdown.SelectedCount);
            Assert.IsFalse(mst.Dropdown.options[0].selected, "Expected options[0].selected to be false");
            Assert.IsFalse(mst.Dropdown.options[1].selected, "Expected options[1].selected to be false");
            Assert.IsTrue(mst.Dropdown.options[2].selected, "Expected options[2].selected to be true");

            yield return mst.Show();

            //
            // Click option 2 to turn it on, turning option 3 off
            //
            yield return mst.ClickOption(mst.Option2);
            yield return mst.Show();

            Assert.AreEqual(1, mst.Dropdown.value);
            Assert.IsTrue(1 == mst.Dropdown.SelectedCount);
            Assert.IsFalse(mst.Dropdown.options[0].selected, "Expected options[0].selected to be false");
            Assert.IsTrue(mst.Dropdown.options[1].selected, "Expected options[1].selected to be true");
            Assert.IsFalse(mst.Dropdown.options[2].selected, "Expected options[2].selected to be false");

        }

        [UnityTest]
        public IEnumerator SetValue()
        {
            var mst = new MultiSelectTests();
            yield return mst.CheckDefaults();

            Assert.AreEqual(0, mst.Dropdown.value, "Expected value to be zero");

            mst.Dropdown.value = 2;
            yield return new WaitForSeconds(0.5f);

            yield return mst.Show();

            Assert.AreEqual(2, mst.Dropdown.value, "Expected value to be 2");
            Assert.IsFalse(mst.Toggle1.isOn, "Expected toggle 1 to be off");
            Assert.IsFalse(mst.Toggle2.isOn, "Expected toggle 2 to be off");
            Assert.IsTrue(mst.Toggle3.isOn, "Expected toggle 3 to be on");
            
        }

        [UnityTest]
        public IEnumerator Events()
        {
            var mst = new MultiSelectTests();
            yield return mst.CheckDefaults();

            //
            // Set up the event handlers
            //
            var deselected = uint.MaxValue;
            mst.Dropdown.onItemDeselected.AddListener((i) =>
            {
                deselected = i;
            });

            var selected = uint.MaxValue;
            mst.Dropdown.onItemSelected.AddListener((i) =>
            {
                selected = i;
            });

            var oldValue = mst.Dropdown.value;
            Assert.AreEqual(0, oldValue, "Expected old value to be 0");
            Assert.IsTrue(mst.Dropdown.SelectedCount == 1, "Expected one item to be selected");

            var newValue = uint.MaxValue;
            mst.Dropdown.onValueChanged.AddListener((i) =>
            {
                newValue = i;
            });

            yield return mst.Show();
            yield return mst.ClickOption(mst.Option2);

            yield return mst.Show();

            Assert.AreEqual(1, mst.Dropdown.value, "Expected value to be 1");
            Assert.IsFalse(mst.Toggle1.isOn, "Expected toggle 1 to be off");
            Assert.IsTrue(mst.Toggle2.isOn, "Expected toggle 2 to be on");
            Assert.IsFalse(mst.Toggle3.isOn, "Expected toggle 3 to be off");

            Assert.AreEqual(0, deselected, "Expected onItemDeselected to be called with index 0");
            Assert.AreEqual(1, selected, "Expected onItemSelected to be called with index 1");
            Assert.AreEqual(1, newValue, "Expected onValueChanged to be called with value 1");
        }

    }

    public class DropdownEventListener
    {
        DropdownEx _dd;

        public bool DeselectedCalled { get; private set; }
        public uint DeselectedIndex { get; private set; }

        public bool SelectedCalled { get; private set; }
        public uint SelectedIndex { get; private set; }

        public bool ValueChangedCalled { get; private set; }
        public uint NewValue { get; private set; }

        public DropdownEventListener(DropdownEx dd)
        {
            _dd = dd;

            _dd.onItemDeselected.AddListener(onItemDeselected);
            _dd.onItemSelected.AddListener(onItemSelected);
            _dd.onValueChanged.AddListener(onValueChanged);

        }


        void onItemDeselected(uint i)
        {
            DeselectedCalled = true;
            DeselectedIndex = i;
        }

        void onItemSelected(uint i)
        {
            SelectedCalled = true;
            SelectedIndex = i;
        }

        void onValueChanged(uint i)
        {
            ValueChangedCalled = true;
            NewValue = i;
        }

    }
}
