
namespace TicketMachineStateMachine
{
    public interface ITicketMachineState
    {
        void SelectTicket(TicketMachine context, decimal price);
        void InsertMoney(TicketMachine context, decimal amount);
        void DispenseTicket(TicketMachine context);
        void CancelTransaction(TicketMachine context);
        void DisplayState(TicketMachine context);
    }

    public class IdleState : ITicketMachineState
    {
        public void SelectTicket(TicketMachine context, decimal price)
        {
            context.TicketPrice = price;
            context.InsertedAmount = 0;
            context.TransitionTo(new WaitingForMoneyState());
            Console.WriteLine($"Билет выбран. Стоимость: {price} руб.");
        }

        public void InsertMoney(TicketMachine context, decimal amount)
        {
            Console.WriteLine("Ошибка: Сначала выберите билет");
        }

        public void DispenseTicket(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Выберите билет перед выдачей");
        }

        public void CancelTransaction(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Нечего отменять");
        }

        public void DisplayState(TicketMachine context)
        {
            Console.WriteLine("Состояние: Idle (ожидание выбора билета)");
        }
    }

    public class WaitingForMoneyState : ITicketMachineState
    {
        public void SelectTicket(TicketMachine context, decimal price)
        {
            context.TicketPrice = price;
            context.InsertedAmount = 0;
            Console.WriteLine($"Выбран новый билет. Стоимость: {price} руб.");
        }

        public void InsertMoney(TicketMachine context, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Ошибка: Некорректная сумма");
                return;
            }

            context.InsertedAmount += amount;
            Console.WriteLine($"Внесено: {amount} руб. (всего: {context.InsertedAmount} руб.)");

            if (context.InsertedAmount >= context.TicketPrice)
            {
                decimal change = context.InsertedAmount - context.TicketPrice;
                context.TransactionChange = change;
                context.TransitionTo(new MoneyReceivedState());
                Console.WriteLine("Достаточно средств! Переход в MoneyReceived");
                if (change > 0)
                    Console.WriteLine($"Сдача: {change} руб.");
            }
            else
            {
                decimal remaining = context.TicketPrice - context.InsertedAmount;
                Console.WriteLine($"Требуется еще: {remaining} руб.");
            }
        }

        public void DispenseTicket(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Недостаточно средств");
        }

        public void CancelTransaction(TicketMachine context)
        {
            Console.WriteLine($"Транзакция отменена. Возвращено: {context.InsertedAmount} руб.");
            context.InsertedAmount = 0;
            context.TicketPrice = 0;
            context.TransactionChange = 0;
            context.TransitionTo(new TransactionCanceledState());
        }

        public void DisplayState(TicketMachine context)
        {
            Console.WriteLine("Состояние: WaitingForMoney (ожидание денег)");
            Console.WriteLine($"  Требуется: {context.TicketPrice} руб.");
            Console.WriteLine($"  Внесено: {context.InsertedAmount} руб.");
        }
    }

    public class MoneyReceivedState : ITicketMachineState
    {
        public void SelectTicket(TicketMachine context, decimal price)
        {
            Console.WriteLine("Ошибка: Завершите текущую транзакцию");
        }

        public void InsertMoney(TicketMachine context, decimal amount)
        {
            Console.WriteLine("Ошибка: Деньги уже получены");
        }

        public void DispenseTicket(TicketMachine context)
        {
            PrintTicket();
            context.TicketsDispensed++;
            Console.WriteLine($"Билет выдан! (Всего: {context.TicketsDispensed})");
            context.TransitionTo(new TicketDispensedState());
        }

        public void CancelTransaction(TicketMachine context)
        {
            Console.WriteLine($"Транзакция отменена. Возвращено: {context.InsertedAmount} руб.");
            context.InsertedAmount = 0;
            context.TicketPrice = 0;
            context.TransactionChange = 0;
            context.TransitionTo(new TransactionCanceledState());
        }

        public void DisplayState(TicketMachine context)
        {
            Console.WriteLine("Состояние: MoneyReceived (деньги получены)");
            Console.WriteLine($"  Цена: {context.TicketPrice} руб.");
            Console.WriteLine($"  Внесено: {context.InsertedAmount} руб.");
            Console.WriteLine("  Готово к выдаче билета");
        }

        private void PrintTicket()
        {
            Console.WriteLine("  [Печать билета...]");
        }
    }

    public class TicketDispensedState : ITicketMachineState
    {
        public void SelectTicket(TicketMachine context, decimal price)
        {
            context.TicketPrice = price;
            context.InsertedAmount = 0;
            context.TransactionChange = 0;
            context.TransitionTo(new WaitingForMoneyState());
            Console.WriteLine($"Новый билет выбран. Стоимость: {price} руб.");
        }

        public void InsertMoney(TicketMachine context, decimal amount)
        {
            Console.WriteLine("Ошибка: Сначала выберите билет");
        }

        public void DispenseTicket(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Билет уже выдан");
        }

        public void CancelTransaction(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Транзакция завершена");
        }

        public void DisplayState(TicketMachine context)
        {
            Console.WriteLine("Состояние: TicketDispensed (билет выдан)");
            Console.WriteLine($"  Цена билета: {context.TicketPrice} руб.");
            if (context.TransactionChange > 0)
                Console.WriteLine($"  Сдача: {context.TransactionChange} руб.");
            
            ReturnToIdle(context);
        }

        private void ReturnToIdle(TicketMachine context)
        {
            System.Threading.Thread.Sleep(500);
            context.TransitionTo(new IdleState());
            Console.WriteLine("  Автомат вернулся в режим ожидания");
        }
    }

    public class TransactionCanceledState : ITicketMachineState
    {
        public void SelectTicket(TicketMachine context, decimal price)
        {
            context.TicketPrice = price;
            context.InsertedAmount = 0;
            context.TransactionChange = 0;
            context.TransitionTo(new WaitingForMoneyState());
            Console.WriteLine($"Билет выбран. Стоимость: {price} руб.");
        }

        public void InsertMoney(TicketMachine context, decimal amount)
        {
            Console.WriteLine("Ошибка: Сначала выберите билет");
        }

        public void DispenseTicket(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Выберите билет для покупки");
        }

        public void CancelTransaction(TicketMachine context)
        {
            Console.WriteLine("Ошибка: Нечего отменять");
        }

        public void DisplayState(TicketMachine context)
        {
            Console.WriteLine("Состояние: TransactionCanceled (отмена)");
            Console.WriteLine("  Средства возвращены");
            
            ReturnToIdle(context);
        }

        private void ReturnToIdle(TicketMachine context)
        {
            System.Threading.Thread.Sleep(500);
            context.TransitionTo(new IdleState());
            Console.WriteLine("  Автомат вернулся в режим ожидания");
        }
    }

    public class TicketMachine
    {
        private ITicketMachineState _currentState;

        public decimal TicketPrice { get; set; }
        public decimal InsertedAmount { get; set; }
        public decimal TransactionChange { get; set; }
        public int TicketsDispensed { get; set; }

        public TicketMachine()
        {
            _currentState = new IdleState();
            TicketPrice = 0;
            InsertedAmount = 0;
            TransactionChange = 0;
            TicketsDispensed = 0;
        }

        public void TransitionTo(ITicketMachineState newState)
        {
            _currentState = newState;
        }

        public void SelectTicket(decimal price)
        {
            _currentState.SelectTicket(this, price);
        }

        public void InsertMoney(decimal amount)
        {
            _currentState.InsertMoney(this, amount);
        }

        public void DispenseTicket()
        {
            _currentState.DispenseTicket(this);
        }

        public void CancelTransaction()
        {
            _currentState.CancelTransaction(this);
        }

        public void DisplayCurrentState()
        {
            Console.WriteLine();
            _currentState.DisplayState(this);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TicketMachine ticketMachine = new TicketMachine();
            
            Console.WriteLine("=== Автомат по продаже билетов ===\n");
            
            Console.WriteLine("--- Сценарий 1: Успешная покупка билета ---");
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.SelectTicket(50);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.InsertMoney(30);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.InsertMoney(25);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.DispenseTicket();
            ticketMachine.DisplayCurrentState();
            
            Console.WriteLine("\n--- Сценарий 2: Отмена транзакции ---");
            ticketMachine.SelectTicket(100);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.InsertMoney(50);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.CancelTransaction();
            ticketMachine.DisplayCurrentState();
            
            Console.WriteLine("\n--- Сценарий 3: Повторная покупка ---");
            ticketMachine.SelectTicket(75);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.InsertMoney(75);
            ticketMachine.DisplayCurrentState();
            
            ticketMachine.DispenseTicket();
            ticketMachine.DisplayCurrentState();
            
            Console.WriteLine("\nПрограмма завершена.");
        }
    }
}
