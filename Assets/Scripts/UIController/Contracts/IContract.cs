using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts
{
    public interface IContract
    {
        /// <summary>
        /// Struct for todo list items.
        /// </summary>
        public class Todo
        {
            public delegate bool BoolDelegate();

            public string Text;
            public BoolDelegate IsFulfilled;

            public Todo(string text, BoolDelegate isFulfilled)
            {
                if (text is null)
                {
                    throw new ArgumentNullException(nameof(text));
                }

                if (isFulfilled is null)
                {
                    throw new ArgumentNullException(nameof(isFulfilled));
                }
                Text = text;
                IsFulfilled = isFulfilled;
            }
        }
        /// <summary>
        /// Unique id for an instance.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Whether the contract was is running.
        /// </summary>
        public bool Running { get; set; }
        /// <summary>
        /// Whether the contract is fulfilled.
        /// </summary>
        public bool Fulfilled { get; set; }
        /// <summary>
        /// The title of the contract.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// The text of the contract that is shown in the contracts screen.
        /// </summary>
        public string Text { get; }
        /// <summary>
        /// List of Todo items that need to be fulfilled.
        /// </summary>
        public Todo[] TodoItems { get; }
        /// <summary>
        /// This method is called when the contract is accepted by the player (started).
        /// </summary>
        public void OnStart();
        /// <summary>
        /// This method checks whether the contract is ready to be fulfilled by the player.
        /// 
        /// This is used to indicate to the player whether a contract is done.
        /// </summary>
        /// <returns>Whether the player has met all requirements to fulfill the contract.</returns>
        public bool CanFulfill()
        {
            bool can = true;
            foreach (Todo todo in TodoItems)
                can &= todo.IsFulfilled();
            return can;
        }
        /// <summary>
        /// This method is called when the player fulfills/finishes the contract by pressing a button.
        /// </summary>
        public void OnFulfill();
        /// <summary>
        /// This method is called when the player aborts the contract.
        /// </summary>
        public void OnAbort();

        /// <summary>
        /// This method is called when the player "hacks" the contract in the contracts screen.
        /// The method must result in <see cref="CanFulfill"/> evaluating to <c>true</c> and
        /// the game being in the same state as if the player actually fulfilled the contract manually.
        /// </summary>
        public void Hack();

        /// <summary>
        /// This method is called when the contract should be serialized as the game is being saved.
        /// </summary>
        /// <returns>A string that can be used in <see cref="Deserialize(string)"/> to restore this instance.</returns>
        public string Serialize();

        /// <summary>
        /// This method is called when a saved game is loaded.
        /// </summary>
        /// <param name="serialized">The string previously returned by <see cref="Serialize"/>.</param>
        public void Deserialize(string serialized);
    }
}