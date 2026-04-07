using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Inventory : Node2D
{
    private static int SlotSize = 60;
    private ItemContainer[] Items { get; set; } = new ItemContainer[SlotSize];
    private int _selectedSlotIndex = -1;
    public ItemContainer CurrentItem => (Items != null && _selectedSlotIndex >= 0) ? Items[_selectedSlotIndex] : null;
    private GridContainer _grid;
    private List<Panel> _slots = new List<Panel>();
    private List<TextureRect> _icons = new List<TextureRect>();
    private List<Label> _labels = new List<Label>();

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("PanelContainer/GridContainer");
        CreateSlots();
    }

    private void CreateSlots()
    {
        for (int i = 0; i < SlotSize; i++)
        {
            // 1. Create Slot
            Panel slotPanel = new Panel();
            slotPanel.CustomMinimumSize = new Vector2(40, 40);
            
            // 2. Create Icon
            TextureRect icon = new TextureRect();
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            
            // 3. Create Label
            Label label = new Label();
            label.HorizontalAlignment = HorizontalAlignment.Right;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            label.AddThemeFontSizeOverride("font_size", 10);

            slotPanel.AddChild(icon);
            slotPanel.AddChild(label);
            _grid.AddChild(slotPanel);

            int index = i;
            slotPanel.GuiInput += (inputEvent) => OnSlotGuiInput(inputEvent, index);

            _slots.Add(slotPanel);
            _icons.Add(icon);
            _labels.Add(label);
        }
    }

    private void OnSlotGuiInput(InputEvent @event, int index)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed)
        {
            if (mouseBtn.ButtonIndex == MouseButton.Left)
            {
                SelectSlot(index);
            }
        }
    }

    private void SelectSlot(int index)
    {
        _selectedSlotIndex = index;
        GD.Print($"Selected Slot: {index} | Item: {(Items[index]?.ItemName.ToString() ?? "Empty")}");
        UpdateUI();
    }

    public BiomeElementNames? UseCurrentItem()
    {
        if (CurrentItem == null || CurrentItem.IsEmpty())
        {
            return null;
        }

        var blockName = CurrentItem.ItemName;
        CurrentItem.CurrentStackSize--;

        if (CurrentItem.IsEmpty())
        {
            Items[_selectedSlotIndex] = null;
        }

        UpdateUI();
        return blockName;
    }

    public void AddNewItem(BiomeElementNames block)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] is not null && Items[i].ItemName == block)
            {
                Items[i].CurrentStackSize++;
                UpdateUI();
                return;
            }
        }

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] is null)
            {
                // Load the resource and DUPLICATE it so stack sizes don't sync across slots
                var res = GD.Load<ItemContainer>($"res://Resources/Items/{block.ToString()}.tres");
                if (res is not null)
                {
                    Items[i] = (ItemContainer)res.Duplicate();
                    Items[i].CurrentStackSize = 1;
                    
                    // Auto-select the first item picked up if nothing is selected
                    if (_selectedSlotIndex == -1) _selectedSlotIndex = i;
                }
                UpdateUI();
                return;
            }
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < SlotSize; i++)
        {
            if (Items[i] is not null)
            {
                // _icons[i].Texture = Items[i].Icon;
                _labels[i].Text = Items[i].CurrentStackSize > 1 ? Items[i].CurrentStackSize.ToString() : "";
            }
            else
            {
                _icons[i].Texture = null;
                _labels[i].Text = "";
            }

            _slots[i].SelfModulate = (i == _selectedSlotIndex) ? new Color(1, 1, 0) : new Color(1, 1, 1);
        }
    }
}