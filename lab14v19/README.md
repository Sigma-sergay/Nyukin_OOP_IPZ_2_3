# Лабораторна робота №14 — Аналіз SRP/OCP у відкритих репозиторіях

**Виконав:** Нюкін Сергій, ІПЗ-3/2

## Мета роботи

Дослідити, як принципи SRP (Single Responsibility) та OCP (Open/Closed) застосовуються в реальних проєктах, і підготувати короткий практичний аналіз прикладів рефакторингу.

## Обрані репозиторії

| № | Репозиторій | Домен | Зафіксований коміт |
|---|-------------|-------|--------------------|
| 1 | [jbogard/MediatR](https://github.com/jbogard/MediatR) | бібліотека-посередник (mediator) для обміну запитами й повідомленнями всередині застосунку | [`1fad4c7`](https://github.com/jbogard/MediatR/tree/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b) |
| 2 | [nopSolutions/nopCommerce](https://github.com/nopSolutions/nopCommerce) | платформа інтернет-магазину (e-commerce) | [`3a14462`](https://github.com/nopSolutions/nopCommerce/tree/3a1446238dfe743537d564d82cae4cd785697c84) |

Усі посилання нижче ведуть на конкретний коміт (permalink), тому номери рядків не «з'їдуть» після нових змін у репозиторіях.

---

## 1. Вдале застосування SRP

### 1.1. MediatR — `RequestPreProcessorBehavior<TRequest, TResponse>`

[`src/MediatR/Pipeline/RequestPreProcessorBehavior.cs`, рядки 12–29](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/Pipeline/RequestPreProcessorBehavior.cs#L12-L29)

```csharp
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
{
    foreach (var processor in _preProcessors)
        await processor.Process(request, cancellationToken).ConfigureAwait(false);

    return await next(cancellationToken).ConfigureAwait(false);
}
```

- **Де видно принцип:** клас має рівно одну відповідальність: перед обробкою запиту запустити всі зареєстровані `IRequestPreProcessor<TRequest>` і передати керування далі. Сам він не валідує, не логує й не обробляє запит, бо це роблять окремі препроцесори.
- **Переваги/ризики:** клас займає менше 30 рядків, його легко протестувати з фейковими препроцесорами, а причина змінитися в нього одна: зміна порядку виклику препроцесорів. Ризиків для підтримки практично немає.
- **Нова функція:** якщо потрібна нова передобробка (наприклад, нормалізація рядків), пишуть новий `IRequestPreProcessor`, а цей клас не змінюється.

### 1.2. nopCommerce — `EncryptionService`

[`src/Libraries/Nop.Services/Security/EncryptionService.cs`, рядки 11–156](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Security/EncryptionService.cs#L11-L156)

Публічний API складається з 4 методів: `CreateSaltKey` (р. 92), `CreatePasswordHash` (р. 110), `EncryptText` (р. 121), `DecryptText` (р. 141). Єдина залежність — `SecuritySettings` (р. 15).

- **Де видно принцип:** клас відповідає лише за криптографічні операції. Він нічого не знає про клієнтів, замовлення чи базу даних: отримує рядок і повертає хеш або шифротекст.
- **Переваги/ризики:** зміна алгоритму шифрування зачіпає тільки цей файл. Сервіс легко повторно використовувати (паролі, номери карток, налаштування) і легко покрити тестами.
- **Нова функція:** щоб додати новий формат хешування, змінюють лише `CreatePasswordHash`. Бізнес-сервіси, які ним користуються, не змінюються.

---

## 2. Порушення SRP

### 2.1. nopCommerce — `OrderProcessingService` (найгірший приклад)

[`src/Libraries/Nop.Services/Orders/OrderProcessingService.cs`](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Orders/OrderProcessingService.cs)

Факти з коду:

- файл має **3564 рядки**;
- клас оголошено в [рядку 40](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Orders/OrderProcessingService.cs#L40);
- [рядки 44–93](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Orders/OrderProcessingService.cs#L44-L93) містять **50 залежностей**, які передаються в конструктор (р. 99);
- понад **40 публічних методів** і 30 захищених.

Публічні методи належать до зовсім різних областей:

| Відповідальність | Методи (рядок) |
|------------------|----------------|
| Оформлення замовлення | `PlaceOrderAsync` (1594), `ReOrderAsync` (3121), `ValidateMinOrderTotalAmountAsync` (3217) |
| Платежі | `CaptureAsync` (2499), `RefundAsync` (2645), `PartiallyRefundAsync` (2829), `VoidAsync` (3009), `MarkOrderAsPaidAsync` (2587) |
| Рекурентні платежі | `ProcessNextRecurringPaymentAsync` (1891), `CancelRecurringPaymentAsync` (2080), `GetNextPaymentDateAsync` (3261) |
| Доставка | `ShipAsync` (2236), `ReadyForPickupAsync` (2285), `DeliverAsync` (2318) |
| Повернення | `IsReturnRequestAllowedAsync` (3157) |

- **Де видно порушення:** у класу щонайменше п'ять причин змінитися: правила оформлення, платежі, підписки, доставка, повернення. 50 залежностей у конструкторі — найкращий індикатор «God Object».
- **Ризики:** зміна логіки повернення коштів може зламати оформлення замовлення, бо вони в одному класі з однаковими залежностями. Юніт-тест будь-якого методу вимагає замокати десятки сервісів. Паралельна робота кількох розробників над цим файлом постійно дає merge-конфлікти.
- **Нова функція:** щоб додати, наприклад, часткову доставку, доведеться правити величезний клас і, ймовірно, додати 51-шу залежність.

### 2.2. MediatR — ліцензування в конфігурації DI та в самому `Mediator`

[`src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs`](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs) має 537 рядків. Один клас `MediatRServiceConfiguration` поєднує:

- налаштування сканування збірок: `RegisterServicesFromAssembly…` ([р. 127–155](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs#L127-L155));
- реєстрацію чотирьох різних видів розширень: `AddBehavior`, `AddStreamBehavior`, `AddRequestPreProcessor`, `AddRequestPostProcessor` (р. 170–513);
- технічні ліміти generic-реєстрацій (`MaxGenericTypeParameters`, `MaxTypesClosing`, `RegistrationTimeout`, [р. 83–98](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs#L83-L98));
- **ліцензійний ключ**: `LicenseKey` і `LicenseKeyFactory` ([р. 108–120](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs#L108-L120)).

Ліцензування є і в класі-посереднику: статична властивість `Mediator.LicenseKey` ([`Mediator.cs`, р. 48](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/Mediator.cs#L48)), хоча сам `Mediator` має лише відправляти запити (`Send`, р. 50) і публікувати сповіщення (`Publish`, р. 121).

- **Де видно порушення:** бізнес-питання ліцензування змішане з технічною відповідальністю (реєстрація в DI, маршрутизація запитів).
- **Ризики:** зміна ліцензійної моделі змушує правити ядро бібліотеки (`Mediator`) і клас конфігурації, яким користуються всі. Статичний стан у `Mediator` ускладнює тести, де потрібні різні ключі.
- **Нова функція:** новий тип розширення pipeline знову додасть 4–5 методів `Add…` у той самий клас.

---

## 3. Розширення за OCP

### 3.1. MediatR — pipeline behaviors і стратегії публікації

**Pipeline behaviors.** Інтерфейс [`IPipelineBehavior<TRequest, TResponse>`, рядки 20–30](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/IPipelineBehavior.cs#L20-L30):

```csharp
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}
```

Кожен behavior огортає обробник і викликає `next`. Логування, валідацію, транзакції чи кешування додають **новим класом**, який реалізує інтерфейс і реєструється через `AddBehavior`. Код `Mediator` і наявних обробників не змінюється. Приклад вбудованого розширення — `RequestPreProcessorBehavior` з пункту 1.1.

**Стратегії публікації.** Інтерфейс [`INotificationPublisher`, рядки 7–11](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/INotificationPublisher.cs#L7-L11) має дві реалізації:

- [`ForeachAwaitPublisher`](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/NotificationPublishers/ForeachAwaitPublisher.cs#L15-L24) викликає обробники послідовно;
- [`TaskWhenAllPublisher`](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/NotificationPublishers/TaskWhenAllPublisher.cs#L18-L28) викликає їх паралельно через `Task.WhenAll`.

Стратегію задають у конфігурації (`NotificationPublisher`, [р. 28](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/MicrosoftExtensionsDI/MediatrServiceConfiguration.cs#L28)), і `Mediator` отримує її через конструктор ([р. 37](https://github.com/jbogard/MediatR/blob/1fad4c7a35c8dd4e733a0f8d43e28eddc1aa5f5b/src/MediatR/Mediator.cs#L37)).

- **Переваги:** нова поведінка (наприклад, публікація з retry чи з обмеженням паралелізму) — це новий клас без зміни бібліотеки.
- **Ризики:** при великій кількості behaviors важко відстежити порядок їх виконання, і кожен з них додає накладні витрати на кожен запит.

### 3.2. nopCommerce — плагіни доставки, податків і оплати

Ядро магазину працює з абстракціями, а конкретні служби є плагінами:

- [`IShippingRateComputationMethod : IPlugin`, рядки 9–39](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Shipping/IShippingRateComputationMethod.cs#L9-L39): `GetShippingOptionsAsync`, `GetFixedRateAsync`, `GetShipmentTrackerAsync`;
- [`ITaxProvider : IPlugin`, рядки 8–29](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Tax/ITaxProvider.cs#L8-L29): `GetTaxRateAsync`, `GetTaxTotalAsync`;
- [`IPaymentMethod`](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Payments/IPaymentMethod.cs): `ProcessPaymentAsync` (р. 22), `RefundAsync` (р. 69) тощо. Реалізацію, наприклад [`CheckMoneyOrderPaymentProcessor`](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Plugins/Nop.Plugin.Payments.CheckMoneyOrder/CheckMoneyOrderPaymentProcessor.cs), підвантажує [`PaymentPluginManager : PluginManager<IPaymentMethod>`](https://github.com/nopSolutions/nopCommerce/blob/3a1446238dfe743537d564d82cae4cd785697c84/src/Libraries/Nop.Services/Payments/PaymentPluginManager.cs#L12).

- **Переваги:** підключити «Нову пошту» чи новий платіжний шлюз можна окремим проєктом-плагіном, не змінюючи `Nop.Services`. Плагіни вмикаються й вимикаються в адмінці.
- **Ризики:** інтерфейс `IPaymentMethod` дуже широкий (14 методів і 7 властивостей). Кожен новий плагін мусить реалізувати навіть непотрібні методи, наприклад `VoidAsync` для оплати чеком. Це вже порушення ISP, і будь-яка зміна інтерфейсу ламає всі наявні плагіни.

---

## 4. Пропозиція рефакторингу для найгіршого прикладу (`OrderProcessingService`)

Розділити клас за відповідальностями (областями зміни), залишивши фасад для зворотної сумісності:

```
IOrderProcessingService (фасад, лише делегує)
├── IOrderPlacementService      — PlaceOrderAsync, ReOrderAsync, ValidateMin…Amount
├── IOrderPaymentService        — CaptureAsync, RefundAsync, PartiallyRefundAsync, VoidAsync, MarkOrderAsPaidAsync
├── IRecurringPaymentService    — ProcessNextRecurringPaymentAsync, CancelRecurringPaymentAsync, GetNextPaymentDateAsync
├── IShipmentProcessingService  — ShipAsync, ReadyForPickupAsync, DeliverAsync
└── IOrderStatusService         — CheckOrderStatusAsync, CancelOrderAsync, Can… перевірки
```

Покроковий план:

1. Виділити методи однієї групи (наприклад, доставки) в новий клас і перенести в нього **лише ті залежності, які він реально використовує**.
2. У старому `OrderProcessingService` замінити тіла цих методів на делегування новому сервісу. Публічний API (`IOrderProcessingService`) лишається тим самим, тож плагіни й контролери не ламаються.
3. Повторити для інших груп. Пари `CanXxx` / `XxxAsync` (`CanRefundAsync` + `RefundAsync`) переносити разом.
4. Коли всі групи винесено, позначити фасад `[Obsolete]` і поступово перевести виклики на нові інтерфейси.

**Обґрунтування:** кожен новий клас матиме 5–12 залежностей замість 50, і його можна тестувати окремо. Зміна в поверненні коштів більше не зачіпає оформлення замовлення. Нова функція (наприклад, часткова доставка) додається в `IShipmentProcessingService` без ризику для платежів.

---

## Висновки

1. Обидва проєкти широко застосовують OCP: MediatR через pipeline behaviors і стратегії `INotificationPublisher`, nopCommerce через систему плагінів (`IPlugin`). Завдяки цьому нову поведінку додають новими класами, не змінюючи ядро.
2. SRP найкраще дотримано в невеликих класах з одною залежністю й вузьким API (`RequestPreProcessorBehavior`, `EncryptionService`).
3. Найсерйозніше порушення SRP — `OrderProcessingService` у nopCommerce: 3564 рядки й 50 залежностей. Це типовий «God Object», що виріс разом із доменом.
4. У MediatR порушення менш помітне: бізнес-питання ліцензування проникло в технічні класи (`Mediator.LicenseKey`, `MediatRServiceConfiguration.LicenseKey`).
5. OCP і SRP пов'язані: точки розширення (плагіни) в nopCommerce добре ізольовані, а сервіс, що їх оркеструє, акумулював усю решту логіки.
6. Надмірно широкий інтерфейс розширення (`IPaymentMethod`) показує, що OCP без ISP створює нові проблеми для авторів плагінів.
7. Головний практичний індикатор порушення SRP — кількість залежностей у конструкторі та кількість груп методів, які змінюються з різних причин, а не просто розмір файлу.

## Відповіді на контрольні запитання

### 1. Як відрізнити окрему відповідальність класу від просто великого обсягу коду?

Відповідальність — це **причина змінитися** (або «актор», якому потрібна зміна). Клас може бути великим, але мати одну відповідальність, наприклад складний алгоритм шифрування чи парсер. Також він може бути малим, але змішувати кілька. Ознаки окремих відповідальностей:

- групи методів, які використовують **різні** підмножини полів чи залежностей;
- методи, які змінюються з різних бізнес-причин і на прохання різних людей (бухгалтерія, склад, служба доставки);
- можливість назвати групу методів окремим іменником («доставка», «повернення»), і клас при цьому не втрачає сенсу.

`OrderProcessingService` поганий не тому, що має 3564 рядки, а тому, що доставку, платежі й підписки змінюють з різних причин.

### 2. Які ознаки в коді найкраще підтверджують дотримання OCP?

- Є **абстракція** (інтерфейс чи абстрактний клас) у точці, де очікуються варіації: `IPipelineBehavior`, `IShippingRateComputationMethod`.
- Нова поведінка додається **новим класом**, а не новою гілкою `if/switch` у наявному коді.
- Реалізації підставляються через DI, конфігурацію або механізм плагінів, тобто виконавчий код не знає конкретних типів.
- Існує кілька реалізацій однієї абстракції (`ForeachAwaitPublisher` і `TaskWhenAllPublisher`). Це доводить, що точка розширення реально працює.
- В історії комітів додавання функцій виглядає як додавання файлів, а не як редагування ядра.

### 3. Чому для аналізу SRP/OCP важливо дивитися не лише на інтерфейси, а й на місця розширення системи?

Інтерфейс сам по собі ще не означає OCP. Важливо, **хто і як** ним користується. Якщо в точці виклику стоїть `if (method is PayPal) … else if (method is Stripe) …`, то інтерфейс є, а закритості для змін немає. Саме в місцях розширення (реєстрація в DI, менеджери плагінів, оркестратори) видно, чи нову реалізацію справді можна додати без правок. Там же часто накопичується порушення SRP: у nopCommerce плагіни ізольовані, але `OrderProcessingService`, який їх викликає, перебрав на себе всю координацію. Крім того, ширина інтерфейсу видна лише при спробі написати нове розширення (`IPaymentMethod` з 21 членом).

### 4. Які ризики виникають, якщо застосовувати рефакторинг «під патерн» без реальної потреби?

- **Надмірна складність:** десятки інтерфейсів з однією реалізацією, фабрики фабрик. Код стає важче читати, ніж до рефакторингу.
- **Хибні абстракції:** точку розширення зробили там, де варіацій ніколи не буде, а там, де вони з'являться, її немає. Згодом абстракцію доводиться ламати.
- **Час і ризик регресій:** зміна працюючого коду без бізнес-причини може внести помилки, особливо без тестів.
- **Падіння продуктивності:** зайві шари непрямих викликів і алокацій (кожен pipeline behavior додає витрати на кожен запит).
- **Порушення YAGNI/KISS:** команда підтримує код «на майбутнє», яке не настає.

Рефакторинг варто робити, коли з'являється реальний сигнал: друга схожа реалізація, часті конфлікти в одному файлі, неможливість протестувати клас.
