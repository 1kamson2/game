using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Inventory : HBoxContainer
{
    private static int _baseInventoryCapacity = 60;
    private static int _baseRecipeCapacity = 128;
    private int _currentInventoryCapacity = _baseInventoryCapacity;
    private int _selectedInventorySlot = -1;
    private int _selectedRecipeSlot = -1;
    public List<IInventoryContainer> Items { get; set; } = new List<IInventoryContainer>(_baseInventoryCapacity);
    public IInventoryContainer CurrentItem => (Items != null && _selectedInventorySlot >= 0) ? Items[_selectedInventorySlot] : null;

    // Definitions for storing items in the inventory
    private GridContainer _inventoryGrid;
    private List<Panel> _inventorySlots = new List<Panel>();
    private List<TextureRect> _inventoryIcons = new List<TextureRect>();
    private List<Label> _inventoryLabels = new List<Label>();

    // Definitions for recipes
    private GridContainer _recipeGrid;
    public List<UsableItem> Recipes { get; set; } = new List<UsableItem>(_baseRecipeCapacity);
    private List<Panel> _recipeSlots = new List<Panel>(_baseRecipeCapacity);
    private List<TextureRect> _recipeIcons = new List<TextureRect>(_baseRecipeCapacity);
    private List<Label> _recipeLabels = new List<Label>(_baseRecipeCapacity);

    public override void _Ready()
    {
        _inventoryGrid = GetNode<GridContainer>("Items/Grid");
        _recipeGrid = GetNode<GridContainer>("Recipes/ScrollContainer/Grid");
        InitializeInventorySlots();
        InitializeRecipeSlots();
    }

    private void InitializeRecipeSlots()
    {
        for (int i = 0; i < _baseRecipeCapacity; i++)
        {
            // 1. Create Slot
            Panel slotPanel = new Panel();
            slotPanel.CustomMinimumSize = new Vector2(32, 32);

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
            _recipeGrid.AddChild(slotPanel);

            int index = i;
            slotPanel.GuiInput += (inputEvent) => OnSlotGuiInput(inputEvent, index + _baseInventoryCapacity);


            _recipeSlots.Add(slotPanel);
            _recipeIcons.Add(icon);
            _recipeLabels.Add(label);
            Recipes.Add(null);
        }
    }

    private void InitializeInventorySlots()
    {
        for (int i = 0; i < _currentInventoryCapacity; i++)
        {
            // 1. Create Slot
            Panel slotPanel = new Panel();
            slotPanel.CustomMinimumSize = new Vector2(32, 32);

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
            _inventoryGrid.AddChild(slotPanel);

            int index = i;
            slotPanel.GuiInput += (inputEvent) => OnSlotGuiInput(inputEvent, index);

            _inventorySlots.Add(slotPanel);
            _inventoryIcons.Add(icon);
            _inventoryLabels.Add(label);
            Items.Add(null);
        }
    }

    public string UseCurrentItem()
    {
        if (CurrentItem == null || CurrentItem.IsEmpty())
        {
            return null;
        }

        var blockId = CurrentItem.Id;
        CurrentItem.CurrentStackSize--;
        UpdateInventoryState();
        return blockId;
    }

    public void AddNewBlock(Block block)
    {
        // Check if container already exists
        int itemIndex = -1;
        itemIndex = Items.FindIndex(item =>
            item is not null &&
            item.Name == block.Name &&
            item.CanBeStacked()
        );
        if (itemIndex != -1)
        {
            Items[itemIndex].CurrentStackSize++;
            UpdateInventoryState();
            return;
        }

        itemIndex = Items.FindIndex(item => item is null);
        // Put into already existing container
        if (itemIndex != -1)
        {
            Items[itemIndex] = (IInventoryContainer)block.Duplicate();
            Items[itemIndex].CurrentStackSize = 1;
            string biomeId = GlobalManagers.Instance.GetManager<WorldManager>().CurrentBiome.Id;
            _inventoryIcons[itemIndex].Texture = GlobalManagers.Instance.GetManager<TilesetManager>().GetTileTexture(biomeId, 1, block.TilesetCoordinates);
            _selectedInventorySlot = _selectedInventorySlot == -1 ? itemIndex : _selectedInventorySlot;
            UpdateInventoryState();
            return;
        }
    }

    public void AddNewItem(UsableItem newItem)
    {
        // Check if container already exists
        int itemIndex = -1;
        itemIndex = Items.FindIndex(item =>
            item is not null &&
            item.Name == newItem.Name &&
            item.CanBeStacked()
        );
        if (itemIndex != -1)
        {
            Items[itemIndex].CurrentStackSize++;
            UpdateInventoryState();
            return;
        }

        itemIndex = Items.FindIndex(item => item is null);
        // Put into already existing container
        if (itemIndex != -1)
        {
            Items[itemIndex] = (IInventoryContainer)newItem.Duplicate();
            Items[itemIndex].CurrentStackSize = 1;
            string biomeId = GlobalManagers.Instance.GetManager<WorldManager>().CurrentBiome.Id;
            _inventoryIcons[itemIndex].Texture = GlobalManagers.Instance.GetManager<TilesetManager>().GetTileTexture(biomeId, 1, newItem.TilesetCoordinates);
            _selectedInventorySlot = _selectedInventorySlot == -1 ? itemIndex : _selectedInventorySlot;
            UpdateInventoryState();
            return;
        }
    }

    private void UpdateInventoryState()
    {
        for (int i = 0; i < _currentInventoryCapacity; i++)
        {
            if (Items[i] is not null && !Items[i].IsEmpty())
            {
                _inventoryLabels[i].Text = Items[i].CurrentStackSize.ToString();
            }
            else
            {
                Items[i] = null;
                _inventoryIcons[i].Texture = null;
                _inventoryLabels[i].Text = "";
            }
            _inventorySlots[i].SelfModulate = (i == _selectedInventorySlot) ? new Color(1, 1, 0) : new Color(1, 1, 1);
        }
    }

    private void OnSlotGuiInput(InputEvent @event, int index)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed)
        {
            GD.Print(index);
            if (mouseBtn.ButtonIndex == MouseButton.Left)
            {
                if (index >= _baseInventoryCapacity)
                {
                    _selectedRecipeSlot = index;
                    CraftNewItem();
                }
                else
                {
                    _selectedInventorySlot = index;
                }
            }
        }
    }

    public void CraftNewItem()
    {
        UsableItem recipe = Recipes[_selectedRecipeSlot - _baseInventoryCapacity];

        foreach (var requiredItem in recipe.RequiredItems)
        {
            int index = Items.FindIndex(item =>
                item is not null &&
                item.Id == requiredItem.Id
            );
            Items[index].CurrentStackSize -= requiredItem.Amount;
        }
        // TODO: Add checking if we actually can create new item.
        AddNewItem(recipe);
        UpdateRecipesState();
    }

    public void UpdateRecipesState()
    {
        // TODO: Add signal emitting so there is no reason to call update everytime
        // TODO: Recipes are added at the end of the container
        GlobalManagers.Instance.GetManager<ItemManager>().FindAllCraftableItems(this);
        Recipes.OrderBy(item => item.Id);
        for (int i = 0; i < _baseRecipeCapacity; i++)
        {
            if (Recipes[i] is null)
            {
                continue;
            }
            GD.Print($"This is craftable {Recipes[i].Id}");
            string biomeId = GlobalManagers.Instance.GetManager<WorldManager>().CurrentBiome.Id;
            _recipeIcons[i].Texture = GlobalManagers.Instance.GetManager<TilesetManager>().GetTileTexture(biomeId, 1, Recipes[i].TilesetCoordinates);
            _recipeLabels[i].Text = Recipes[i].Name;
        }
    }
}