# DropdownEx Read Me

Author: Chris Cowherd (madcow@gmail.com)

Clone of the [UnityEngine.UI.Dropdown](https://bitbucket.org/Unity-Technologies/ui/src) component extended optionally giving the user the ability to select multiple options.

The proper thing to do is to modify the original Dropdown so it was properly extensible, then just extend here but that would require a lot of surgery and I need to move on with my real project that I need this for.  Maybe I'll do that in the future or feel free to pitch in.


## Setup

I don't know how to add a prebuilt component to the Context menu of Unity so I will explain how to assemble it by hand.  Feel free to submit a pull request if you know how to do it.

The easiest way to set this up is to just use the included prefab.

To create it from scratch simply add a standard Dropdown component as a child of a Canvas as usual.  This will give you all the necessary child GameObjects and structure. Then simply remove the Dropdown component and add the DropdownEx component.

Next, set the Template property by dragging the Template GameObject into the property box in the inspector.

Next up, set the `Caption Text` property by dragging the first `Label` GameObject child onto the property box.

Then set the `Item Text` property by expanding the `Template/Viewport/Content/Item` hierarchy and dragging the `Item Label` to the `Item Text` property.

Thats it!

## Running the Tests
When you first load the project you will get an error about NUnit.  I didn't include the test assembly folder in the repository figuring you might already have one in your project.  If you don't care about testing (shame shame!), just delete the `Plugins/UnityEngine.UI/UI/Core/DropdownEx/Tests` folder.   To enable testing, go to the `Window > General > Test Runner` menu.  Choose the `PlayMode` button and click `Create PlayMode Test Assembly Folder`.  Then in the Test Runner window, chose the little menu icon in the top right and select `Enable playmode tests for all assemblies` and restart Unity.  Lastly, open the `DropdownEx Test Scene`, go to `File > Build Settings...` and click `Add open scenes`.  You should be able to run the tests now.

## Usage

DropdownEx should behave exactly like the standard Dropdown out of the box.  There are some additional properties and events added and enabling the `MultiSelect` property changes the behavior in some ways.

### Value Property
Without multi-select, the value property is the 0-based index of the selected option. *With multi-select enabled*, the value property is now a set of bit flags representing the selected state of the options, making it 1-based.  This is because a value of 0 means nothing is selected.  A value of 1 means the first option is selected; a value of 2 means the second option is selected.  Finally a value of 3 means *both options 1 and 2 are selected*.

If you programmatically change the state of multi-select, it will reset the value property to 0.

### SelectedCount
Returns the number of selected options. This is handier than counting set bits in DropdownEx::value.

### SelectedOptions
Returns an enumerable that will yield each selected option.

### onItemSelected Event
Fired when an option is selected. The argument is the index into the OptionData array: DropdownEx::options.

### onItemDeselected Event
Fired when an option is deselected. The argument is the index into the OptionData array: DropdownEx::options.



