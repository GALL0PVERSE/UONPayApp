UONPayApp - Separate Accounts, Cards, Local Storage, and Vibration

Test accounts:
1. S1234567 / Pass@123
2. S2345678 / Welcome1
3. S3456789 / Secure99

Main features in this version:
- Each student account has its own balance.
- Each student account has its own bill/payment history.
- Each student account has its own saved payment cards.
- Local storage is saved to uonpayapp-data.json using FileSystem.Current.AppDataDirectory.
- The phone vibrates when the payment success popup appears.

Important files:
- Services/AppDataService.cs
  Main local storage and account data service. Comments explain Load(), Save(), TryLogin(), AddTransaction(), and per-student ledgers.

- Models/AppDataStore.cs
  Root local database object saved as JSON.

- Models/AccountLedger.cs
  Per-student balance and transactions model.

- Models/LoginAccount.cs
  Login account and saved card list.

- Views/PaymentsPage.xaml.cs
  Payment page logic. Comments explain account-specific cards, payment saving, balance refresh, and vibration before popup.

- Platforms/Android/AndroidManifest.xml
  Contains the VIBRATE permission required for Android vibration.

Build APK:
dotnet restore
dotnet publish UONPayApp.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk

APK output folder:
bin/Release/net10.0-android/publish/
