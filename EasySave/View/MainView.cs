using System;
using EasySave.Ressources; // Indispensable pour lier avec l'étape 2

namespace EasySave.Views
{
    public class MainView
    {
        public void ShowMenu()
        {
            Console.Clear();
            // Titre traduit
            Console.WriteLine("========================================");
            Console.WriteLine($"    {Strings.MenuTitle}");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Options traduites
            Console.WriteLine(Strings.MenuOption1);
            Console.WriteLine(Strings.MenuOption2);
            Console.WriteLine(Strings.MenuOption3);
            Console.WriteLine(Strings.MenuOption4);
            Console.WriteLine(Strings.MenuOption5);
            Console.WriteLine(Strings.MenuExit);
            Console.WriteLine();

            // Demande de choix
            Console.Write($"{Strings.AskChoice} ");
        }
    }
}