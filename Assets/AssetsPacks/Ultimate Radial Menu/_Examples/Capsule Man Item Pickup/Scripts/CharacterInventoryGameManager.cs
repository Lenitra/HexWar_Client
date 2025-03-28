﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateRadialMenuExample.CharacterInventory2D
{
	public class CharacterInventoryGameManager : MonoBehaviour
	{
		// TUTORIAL VARIABLES //
		public Text tutorialText;
		bool hasPickedUpItems, hasInteractedWithButton;
		
		// MENU VARIABLES //
		public UltimateRadialMenu radialMenu;
		public GameObject pauseScreen;
		public SpriteRenderer backgroundSprite;

		// ITEM SPAWNING //
		float itemSpawningTimer = 0.0f;
		public float itemSpawningRate = 2.5f;
		Vector2 spawnRangeMin, spawnRangeMax;
		public GameObject itemBasePrefab;

		[System.Serializable]
		public class ItemInformation
		{
			public string name;
			public Sprite itemSprite;
			public int itemCount = 0;
			public UltimateRadialButtonInfo buttonInfo;

			public void UseItem ()
			{
				// THIS IS WHERE YOUR ITEM LOGIC WOULD APPLY THE ITEMS EFFECT!!
				Debug.Log( "Using: " + name );
			}
		}
		public ItemInformation[] items;
		Dictionary<string, ItemInformation> itemDictionary = new Dictionary<string, ItemInformation>();

		void Start ()
		{
			// Change the size of the background to fill up the camera space.
			backgroundSprite.size = new Vector2( ( ( Camera.main.orthographicSize * Screen.width ) / Screen.height ) * 2, Camera.main.orthographicSize * 2 );
			backgroundSprite.transform.position = Vector2.zero;

			// Configure the spawn ranges for the items.
			spawnRangeMin = ( -backgroundSprite.size / 2 ) * 0.95f;
			spawnRangeMax = ( backgroundSprite.size / 2 ) * 0.95f;

			// Set the base item prefab as inactive.
			itemBasePrefab.SetActive( false );

			// Clear the radial menu button so that the menu starts with no items.
			radialMenu.ClearMenu();

			// Loop through each of the items...
			for( int i = 0; i < items.Length; i++ )
			{
				// Apply the item information the radial button info for this item.
				items[ i ].buttonInfo.key = items[ i ].name;
				items[ i ].buttonInfo.name = items[ i ].name;
				items[ i ].buttonInfo.icon = items[ i ].itemSprite;

				// Add the item to the dictionary with the name as the key.
				itemDictionary.Add( items[ i ].name, items[ i ] );

				// Register this item to the radial menu.
				radialMenu.RegisterButton( UseItem, items[ i ].buttonInfo );

				// Disable the button since there is no count.
				items[ i ].buttonInfo.DisableButton();

				// Set the text to nothing so that nothing is displayed.
				items[ i ].buttonInfo.UpdateText( "" );
			}

			// Here we are subscribing to the Enabled and Disabled events so that we can pause and resume the game when the menu changes state.
			radialMenu.OnMenuEnabled += PauseGame;
			radialMenu.OnMenuDisabled += ResumeGame;

			// Set the pause screen to inactive.
			pauseScreen.SetActive( false );

			// This is for the tutorial text only //
			radialMenu.OnButtonInteract += ( buttonIndex ) =>
			{
				if( !hasInteractedWithButton )
					tutorialText.text = "Awesome! When you interact with a radial button, check out the Console to see that the radial button called the right item.";

				hasInteractedWithButton = true;
			};
		}

		void Update ()
		{
			// Increase the spawn timer.
			itemSpawningTimer += Time.deltaTime;

			// If the spawn timer is greater than the spawning rate...
			if( itemSpawningTimer >= itemSpawningRate )
			{
				// Reset the spawning timer.
				itemSpawningTimer -= itemSpawningRate;

				// Instantiate a new item in the game.
				GameObject newItem = Instantiate( itemBasePrefab, new Vector3( Random.Range( spawnRangeMin.x, spawnRangeMax.x ), Random.Range( spawnRangeMin.y, spawnRangeMax.y ) ), Quaternion.identity );

				// Make sure that the item is active in the scene.
				newItem.SetActive( true );

				// Give the new item a random item from the list.
				ItemInformation randomItem = items[ Random.Range( 0, items.Length ) ];
				newItem.GetComponent<SpriteRenderer>().sprite = randomItem.itemSprite;
				newItem.GetComponent<WorldItem>().myManager = this;
				newItem.GetComponent<WorldItem>().myInformation = randomItem;
			}
		}

		/// <summary>
		/// This function is called by the WorldItem that has been picked up.
		/// </summary>
		public void PickupItem ( ItemInformation itemInfo )
		{
			// If the item count is currently at 0, then enable the button since we picked up this item.
			if( itemInfo.itemCount == 0 )
				itemInfo.buttonInfo.EnableButton();

			// Increase the item count.
			itemInfo.itemCount++;
			
			// Update the button text with the new count of the item.
			itemInfo.buttonInfo.UpdateText( itemInfo.itemCount.ToString() );

			// TUTORIAL TEXT ONLY // If the player has not picked up and item yet, update the tutorial text.
			if( !hasPickedUpItems )
			{
				tutorialText.text = "Great! Now press the TAB key to open your radial menu. Then click on the item that you want to use.";
				hasPickedUpItems = true;
			}
		}

		/// <summary>
		/// This function will be called from the radial button when it is interacted with. The itemKey parameter is how this script identifies which item was used.
		/// </summary>
		void UseItem ( string itemKey )
		{
			// If the dictionary does not contain the key sent, then return a warning message.
			if( !itemDictionary.ContainsKey( itemKey ) )
			{
				Debug.LogWarning( "Key does not exist in the dictionary." );
				return;
			}

			// Decrease the item count of the item, and update the radial button text.
			itemDictionary[ itemKey ].itemCount--;
			itemDictionary[ itemKey ].buttonInfo.UpdateText( itemDictionary[ itemKey ].itemCount.ToString() );

			// Call the UseItem() function for this item.
			itemDictionary[ itemKey ].UseItem();

			// If the item count has been depleted...
			if( itemDictionary[ itemKey ].itemCount <= 0 )
			{
				// Disable the button for this item.
				itemDictionary[ itemKey ].buttonInfo.DisableButton();

				// Update the text to nothing so it doesn't appear.
				itemDictionary[ itemKey ].buttonInfo.UpdateText( "" );
			}
		}

		// This function is subscribed to the OnRadialMenuEnabled event.
		void PauseGame ()
		{
			pauseScreen.SetActive( true );
			Time.timeScale = 0.0f;
		}

		// This function is subscribed to the OnRadialMenuDisabled event.
		void ResumeGame ()
		{
			pauseScreen.SetActive( false );
			Time.timeScale = 1.0f;
		}
	}
}