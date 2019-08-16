using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;

namespace CarWashNet.Applications
{
    public enum InteractionResult
    {
        Yes,
        No,
        OK,
        Cancel
    }        
    public class MessageInput
    {
        public string Title { get; private set; } = "Заголовок сообщения";
        public string Message { get; private set; } = "Сообщение";
        public MessageInput(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
    public class ConfirmationInput
    {
        public string Title { get; private set; } = "Заголовок сообщения";
        public string Message { get; private set; } = "Сообщение";
        public string YesString { get; private set; } = "Да";
        public string NoString { get; private set; } = "Нет";
        public ConfirmationInput(string title, string message, string yesString = "Да", string noString = "Нет")
        {
            Title = title;
            Message = message;
            YesString = yesString;
            NoString = noString;
        }
    }

    public static class Interactions
    {
        public static readonly Interaction<MessageInput, InteractionResult> SimpleMessage = new Interaction<MessageInput, InteractionResult>();
        public static readonly Interaction<MessageInput, InteractionResult> CriticalError = new Interaction<MessageInput, InteractionResult>();
        public static readonly Interaction<ConfirmationInput, InteractionResult> Confirmation = new Interaction<ConfirmationInput, InteractionResult>();
        public static readonly Interaction<MessageInput, InteractionResult> LongTimeOperationStarter = new Interaction<MessageInput, InteractionResult>();
        public static readonly Interaction<Unit, InteractionResult> LongTimeOperationFinisher = new Interaction<Unit, InteractionResult>();
        public static readonly Interaction<Unit, InteractionResult> UserLogout = new Interaction<Unit, InteractionResult>();

        public static async Task<InteractionResult> ShowMessage(string title, string message)
        {            
            return await SimpleMessage.Handle(new MessageInput(title, message));
        }
        public static async Task<InteractionResult> ShowError(string message)
        {
            return await ShowMessage("Ошибка", message);
        }        
        public static async Task<InteractionResult> RaiseCriticalError(string message)
        {
            return await CriticalError.Handle(new MessageInput("Критическая ошибка!", message));
        }        
        public static async Task<InteractionResult> ShowConfirmationAsync(string title, string message, string yesString = "Да", string noString = "Нет")
        {
            return await Confirmation.Handle(
                new ConfirmationInput(title, message, yesString, noString));
        }        
        public static async Task<InteractionResult> StartLongTimeOperation(string title, string message)
        {
            return await LongTimeOperationStarter.Handle(new MessageInput(title, message));
        }        
        public static async Task<InteractionResult> FinishLongTimeOperation()
        {
            return await LongTimeOperationFinisher.Handle(Unit.Default);
        }
        public static async Task<InteractionResult> LogoutUser()
        {
            return await UserLogout.Handle(Unit.Default);
        }







        //public static readonly Subject<MessageInput> LongTimeOperationStarter = new Subject<MessageInput>();
        //public static readonly Subject<Unit> LongTimeOperationFinisher = new Subject<Unit>();
        //public static void StartLongTimeOperation(string title, string message)
        //{
        //    LongTimeOperationStarter.OnNext(new MessageInput(title, message));
        //}
        //public static void FinishLongTimeOperation()
        //{
        //    LongTimeOperationFinisher.OnNext(Unit.Default);
        //}




    }






    public enum InteractionType
    {
        SimpleMessage,
        CriticalError,
        Confirmation,
        LongTimeOperationStarter,
        LongTimeOperationFinisher
    }
    public class InteractionInput
    {
        public string Title { get; private set; } = "Заголовок сообщения";
        public string Message { get; private set; } = "Сообщение";
        public string YesString { get; private set; } = "Да";
        public string NoString { get; private set; } = "Нет";
        public InteractionType Type { get; private set; }
        public bool IsLocal { get; private set; }


        public InteractionInput(
            string title,
            string message,
            string yesString,
            string noString,
            InteractionType type,
            bool isLocal)
        {
            Title = title;
            Message = message;
            YesString = yesString;
            NoString = noString;
            Type = type;
            IsLocal = isLocal;
        }
        public InteractionInput(string message)
            : this("Ошибка", message, "Да", "Нет", InteractionType.SimpleMessage, false)
        {
        }
        public InteractionInput(string title, string message)
            : this(title, message, "Да", "Нет", InteractionType.SimpleMessage, false)
        {
        }

    }
}
