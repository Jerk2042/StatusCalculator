using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using static CharacterData;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace StatusCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Hook up the OnTextChanged handler for all existing TextBoxes in the original tabs
            foreach (TabItem tabItem in CharacterTabs.Items)
            {
                StackPanel stackPanel = tabItem.Content as StackPanel;
                if (stackPanel != null)
                {
                    var textBoxes = stackPanel.Children.OfType<TextBox>();
                    foreach (var textBox in textBoxes)
                    {
                        textBox.TextChanged += OnTextChanged; // Attach event handler
                    }
                }
            }
        }





        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if the sender is a TextBox
            var textBox = sender as TextBox;
            if (textBox == null) return; // If not a TextBox, exit.

            // Loop through all tabs to handle text changes
            foreach (TabItem tabItem in CharacterTabs.Items)
            {
                StackPanel stackPanel = tabItem.Content as StackPanel;
                if (stackPanel != null)
                {
                    // Find the ResultTextBlock in the current tab
                    var resultTextBlock = stackPanel.Children.OfType<TextBlock>()
                        .FirstOrDefault(tb => tb.Name == "ResultTextBlock");

                    if (resultTextBlock != null)
                    {
                        // Extract and evaluate input values for calculations
                        double atk = EvaluateExpression(GetTextBoxValue("ATKBox", stackPanel));
                        double hp = EvaluateExpression(GetTextBoxValue("HPBox", stackPanel));
                        double def = EvaluateExpression(GetTextBoxValue("DEFBox", stackPanel));
                        double dmgTakenPerTurn = EvaluateExpression(GetTextBoxValue("DMGTakenPerTurnBox", stackPanel));
                        double dmgTakenThisTurn = EvaluateExpression(GetTextBoxValue("DMGTakenThisTurnBox", stackPanel));
                        double turnCount = EvaluateExpression(GetTextBoxValue("TurnCountBox", stackPanel));

                        // Default to 1 turn if not entered
                        if (turnCount < 1) turnCount = 1;

                        // Calculate the total damage taken after all turns
                        double totalDmgTaken = (dmgTakenPerTurn * turnCount) + dmgTakenThisTurn;
                        double finalDmgTaken = totalDmgTaken - def;

                        // If the final damage taken is less than or equal to 0, set HP to final HP
                        if (finalDmgTaken > 0)
                        {
                            hp -= finalDmgTaken;
                        }

                        // Update the ResultTextBlock with the results
                        resultTextBlock.Text = $"After {turnCount} turn(s):\n" +
                                               $"HP: {hp}\n" +
                                               $"ATK: {atk}\n" +
                                               $"DEF: {def}\n" +
                                               $"Total Damage Taken: {finalDmgTaken}\n";
                    }
                }
            }
        }



        private string GetTextBoxValue(string textBoxName, StackPanel stackPanel)
        {
            // Find the TextBox inside the current tab and return its text value
            var textBox = stackPanel.Children.OfType<TextBox>()
                .FirstOrDefault(tb => tb.Name == textBoxName);
            return textBox?.Text ?? string.Empty;
        }


        private void NextTurn_Click(object sender, RoutedEventArgs e)
        {
            // Increment the Turn Count
            double turnCount = EvaluateExpression(TurnCountBox.Text);
            turnCount++; // Increment by 1
            TurnCountBox.Text = turnCount.ToString(); // Update the TurnCount TextBox

            // Retrieve and evaluate the values from the TextBoxes
            double atk = EvaluateExpression(ATKBox.Text);
            double def = EvaluateExpression(DEFBox.Text);
            double hp = EvaluateExpression(HPBox.Text);
            double dmgTakenThisTurn = EvaluateExpression(DMGTakenThisTurnBox.Text);
            double dmgTakenPerTurn = EvaluateExpression(DMGTakenPerTurnBox.Text);

            // Calculate the total damage taken after all turns
            double totalDmgTaken = (dmgTakenPerTurn * turnCount) + dmgTakenThisTurn;
            double finalDmgTaken = totalDmgTaken - def;

            // If the final damage taken is less than or equal to 0, set HP to final HP
            if (finalDmgTaken > 0)
            {
                hp -= finalDmgTaken;
            }

            // Display Results
            ResultTextBlock.Text = $"After {turnCount} turn(s):\n" +
                                   $"HP: {hp}\n" +
                                   $"ATK: {atk}\n" +
                                   $"DEF: {def}\n" +
                                   $"Total Damage Taken: {finalDmgTaken}\n";
        }

        // Helper method to evaluate expressions and handle errors
        private double EvaluateExpression(string expression)
        {
            // Return 0 if expression is empty, otherwise evaluate it
            if (string.IsNullOrWhiteSpace(expression))
            {
                return 0;
            }

            return ExpressionEvaluator.Evaluate(expression);
        }






        // Event handler for double-clicking a tab to rename it
        private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterTabs.SelectedItem is TabItem selectedTab)
            {
                // Create a TextBox for renaming the tab
                TextBox renameTextBox = new TextBox
                {
                    Text = selectedTab.Header.ToString(), // Set the current tab name as default text
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // When the user finishes editing by pressing Enter or losing focus
                renameTextBox.KeyDown += (s, args) =>
                {
                    // If the Enter key is pressed, finalize the renaming
                    if (args.Key == Key.Enter)
                    {
                        if (!string.IsNullOrEmpty(renameTextBox.Text))
                        {
                            selectedTab.Header = renameTextBox.Text; // Set the new tab name
                        }

                        // Remove focus from the TextBox (finalize the edit)
                        renameTextBox.ClearValue(TextBox.FocusableProperty);
                    }
                };

                renameTextBox.LostFocus += (s, args) =>
                {
                    if (!string.IsNullOrEmpty(renameTextBox.Text))
                    {
                        selectedTab.Header = renameTextBox.Text; // Set the new tab name
                    }
                };

                // Replace the Tab header with the TextBox for editing
                selectedTab.Header = renameTextBox;
                renameTextBox.Focus();
            }
        }


        // Event handler to add a new character tab
        private void AddCharacterTabButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new TabItem
            var newTab = new TabItem
            {
                Header = "New Character", // Default name
            };

            // Create the StackPanel for holding the controls
            StackPanel stackPanel = new StackPanel();

            // Create the TextBox elements for the new tab
            TextBox atkBox = new TextBox { Margin = new Thickness(5), Name = "ATKBox" };
            TextBox defBox = new TextBox { Margin = new Thickness(5), Name = "DEFBox" };
            TextBox hpBox = new TextBox { Margin = new Thickness(5), Name = "HPBox" };
            TextBox dmgTakenPerTurnBox = new TextBox { Margin = new Thickness(5), Name = "DMGTakenPerTurnBox" };
            TextBox dmgTakenThisTurnBox = new TextBox { Margin = new Thickness(5), Name = "DMGTakenThisTurnBox" };
            TextBox turnCountBox = new TextBox { Margin = new Thickness(5), Name = "TurnCountBox" };

            // Add the labels and TextBoxes to the StackPanel
            stackPanel.Children.Add(new TextBlock { Text = "HP:" });
            stackPanel.Children.Add(hpBox);
            stackPanel.Children.Add(new TextBlock { Text = "ATK:" });
            stackPanel.Children.Add(atkBox);
            stackPanel.Children.Add(new TextBlock { Text = "DEF:" });
            stackPanel.Children.Add(defBox);
            stackPanel.Children.Add(new TextBlock { Text = "DMG taken this turn:" });
            stackPanel.Children.Add(dmgTakenThisTurnBox);
            stackPanel.Children.Add(new TextBlock { Text = "DMG taken per turn:" });
            stackPanel.Children.Add(dmgTakenPerTurnBox);
            stackPanel.Children.Add(new TextBlock { Text = "Turn count:" });
            stackPanel.Children.Add(turnCountBox);

            // Add the "Next Turn" button
            Button nextTurnButton = new Button
            {
                Content = "Next Turn",
                Margin = new Thickness(5),
                Width = 200
            };
            nextTurnButton.Click += (s, args) =>
            {
                // Handle the next turn action here
                double turnCount = EvaluateExpression(turnCountBox.Text);
                turnCount++; // Increment by 1
                turnCountBox.Text = turnCount.ToString(); // Update the TurnCount TextBox
            };
            stackPanel.Children.Add(nextTurnButton);

            // Add the ResultTextBlock for displaying the result
            TextBlock resultTextBlock = new TextBlock
            {
                Name = "ResultTextBlock",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 5, 0, 0),
                FontSize = 16
            };
            stackPanel.Children.Add(resultTextBlock);

            // Add the "Delete Tab" button
            Button deleteButton = new Button
            {
                Content = "Delete Tab",
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 0, 0, 0)
            };
            deleteButton.Click += (s, args) =>
            {
                CharacterTabs.Items.Remove(newTab); // Remove the tab
            };
            stackPanel.Children.Add(deleteButton);

            // Bind the OnTextChanged event for each textbox (so result gets updated on input)
            atkBox.TextChanged += OnTextChanged;
            defBox.TextChanged += OnTextChanged;
            hpBox.TextChanged += OnTextChanged;
            dmgTakenThisTurnBox.TextChanged += OnTextChanged;
            dmgTakenPerTurnBox.TextChanged += OnTextChanged;
            turnCountBox.TextChanged += OnTextChanged;

            // Set the content of the new tab
            newTab.Content = stackPanel;

            // Add ContextMenu to the new tab (for renaming)
            ContextMenu tabContextMenu = new ContextMenu();
            MenuItem renameMenuItem = new MenuItem
            {
                Header = "Rename"
            };
            renameMenuItem.Click += RenameMenuItem_Click; // Reuse existing Rename logic
            tabContextMenu.Items.Add(renameMenuItem);
            newTab.ContextMenu = tabContextMenu;

            // Add the new tab to the TabControl
            CharacterTabs.Items.Add(newTab);
        }

        public void SaveApplicationState(string filePath)
        {
            try
            {
                // Gather all the data to save
                var appState = new ApplicationState
                {
                    GlobalMechanicValue = 10, // Replace with your actual mechanic value
                    GameMode = "Trivia",     // Replace with your global state
                };

                foreach (TabItem tab in CharacterTabs.Items)
                {
                    var stackPanel = (StackPanel)tab.Content;

                    var character = new CharacterData
                    {
                        Name = (string)tab.Header,
                        HP = int.Parse(((TextBox)stackPanel.Children[1]).Text),
                        ATK = int.Parse(((TextBox)stackPanel.Children[3]).Text),
                        DEF = int.Parse(((TextBox)stackPanel.Children[5]).Text),
                        DMGTakenThisTurn = int.Parse(((TextBox)stackPanel.Children[7]).Text),
                        DMGTakenPerTurn = int.Parse(((TextBox)stackPanel.Children[9]).Text),
                        TurnCount = int.Parse(((TextBox)stackPanel.Children[11]).Text),
                    };

                    appState.Characters.Add(character);
                }

                // Serialize the application state to a file
                var serializer = new XmlSerializer(typeof(ApplicationState));
                using (var writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, appState);
                }

                MessageBox.Show("Application saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving application: {ex.Message}");
            }
        }



        public void LoadApplicationState(string filePath)
        {
            try
            {
                // Deserialize the XML into the ApplicationState object
                var serializer = new XmlSerializer(typeof(ApplicationState));

                // Ensure the file exists before trying to read it
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("File does not exist.");
                    return;
                }

                using (var reader = new StreamReader(filePath))
                {
                    var appState = (ApplicationState)serializer.Deserialize(reader);

                    // Clear existing tabs before loading
                    CharacterTabs.Items.Clear();

                    // Load the character data into tabs
                    foreach (var characterData in appState.Characters)
                    {
                        AddCharacterTab(characterData);
                    }

                }

                MessageBox.Show("Data Loaded Successfully!");
            }
            catch (InvalidOperationException ex)
            {
                // Handle errors like incorrect XML structure
                MessageBox.Show($"Error loading data: {ex.Message}. Ensure the XML structure matches the expected format.");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                MessageBox.Show($"Unexpected error: {ex.Message}");
            }
        }



        private void AddCharacterTab(CharacterData characterData)
        {
            var newTab = new TabItem
            {
                Header = characterData.Name,
            };

            StackPanel stackPanel = new StackPanel();

            // Add TextBoxes and set their values from characterData
            TextBox atkBox = new TextBox { Margin = new Thickness(5), Name = "ATKBox", Text = characterData.ATK.ToString() };
            TextBox defBox = new TextBox { Margin = new Thickness(5), Name = "DEFBox", Text = characterData.DEF.ToString() };
            TextBox hpBox = new TextBox { Margin = new Thickness(5), Name = "HPBox", Text = characterData.HP.ToString() };
            TextBox dmgTakenPerTurnBox = new TextBox { Margin = new Thickness(5), Name = "DMGTakenPerTurnBox", Text = characterData.DMGTakenPerTurn.ToString() };
            TextBox dmgTakenThisTurnBox = new TextBox { Margin = new Thickness(5), Name = "DMGTakenThisTurnBox", Text = characterData.DMGTakenThisTurn.ToString() };
            TextBox turnCountBox = new TextBox { Margin = new Thickness(5), Name = "TurnCountBox", Text = characterData.TurnCount.ToString() };

            // Add the TextBoxes to the StackPanel
            stackPanel.Children.Add(new TextBlock { Text = "HP:" });
            stackPanel.Children.Add(hpBox);
            stackPanel.Children.Add(new TextBlock { Text = "ATK:" });
            stackPanel.Children.Add(atkBox);
            stackPanel.Children.Add(new TextBlock { Text = "DEF:" });
            stackPanel.Children.Add(defBox);
            stackPanel.Children.Add(new TextBlock { Text = "DMG taken this turn:" });
            stackPanel.Children.Add(dmgTakenThisTurnBox);
            stackPanel.Children.Add(new TextBlock { Text = "DMG taken per turn:" });
            stackPanel.Children.Add(dmgTakenPerTurnBox);
            stackPanel.Children.Add(new TextBlock { Text = "Turn count:" });
            stackPanel.Children.Add(turnCountBox);

            // Add the "Next Turn" button
            Button nextTurnButton = new Button
            {
                Content = "Next Turn",
                Margin = new Thickness(5),
                Width = 200
            };
            nextTurnButton.Click += (s, args) =>
            {
                // Increment turn count
                double turnCount = EvaluateExpression(turnCountBox.Text);
                turnCount++;
                turnCountBox.Text = turnCount.ToString();
            };
            stackPanel.Children.Add(nextTurnButton);

            // Add the ResultTextBlock for displaying the result
            TextBlock resultTextBlock = new TextBlock
            {
                Name = "ResultTextBlock",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 5, 0, 0),
                FontSize = 16
            };
            stackPanel.Children.Add(resultTextBlock);

            // Add the "Delete Tab" button
            Button deleteButton = new Button
            {
                Content = "Delete Tab",
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(5, 0, 0, 0)
            };
            deleteButton.Click += (s, args) =>
            {
                CharacterTabs.Items.Remove(newTab);
            };
            stackPanel.Children.Add(deleteButton);

            // Bind OnTextChanged event
            atkBox.TextChanged += OnTextChanged;
            defBox.TextChanged += OnTextChanged;
            hpBox.TextChanged += OnTextChanged;
            dmgTakenThisTurnBox.TextChanged += OnTextChanged;
            dmgTakenPerTurnBox.TextChanged += OnTextChanged;
            turnCountBox.TextChanged += OnTextChanged;

            // Set the content of the new tab
            newTab.Content = stackPanel;

            // Add ContextMenu for renaming
            ContextMenu tabContextMenu = new ContextMenu();
            MenuItem renameMenuItem = new MenuItem
            {
                Header = "Rename"
            };
            renameMenuItem.Click += RenameMenuItem_Click;
            tabContextMenu.Items.Add(renameMenuItem);
            newTab.ContextMenu = tabContextMenu;

            // Add the new tab
            CharacterTabs.Items.Add(newTab);
        }




        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Save Application State"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveApplicationState(saveFileDialog.FileName);
            }
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Load Application State"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadApplicationState(openFileDialog.FileName);
            }
        }


    }
}
