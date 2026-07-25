export type Language = 'en' | 'am';

export interface Translations {
  common: {
    appName: string;
    loading: string;
    save: string;
    cancel: string;
    delete: string;
    edit: string;
    add: string;
    search: string;
    actions: string;
    status: string;
    active: string;
    inactive: string;
    yes: string;
    no: string;
    confirm: string;
    success: string;
    error: string;
    warning: string;
    backToLogin: string;
    signIn: string;
    rememberPassword: string;
  };
  auth: {
    login: string;
    email: string;
    password: string;
    forgotPassword: string;
    resetPassword: string;
    sendResetLink: string;
    newPassword: string;
    confirmPassword: string;
    passwordChanged: string;
    passwordResetSent: string;
    invalidToken: string;
    logout: string;
  };
  dashboard: {
    title: string;
    totalMedicines: string;
    totalInventoryValue: string;
    todaySales: string;
    monthlySales: string;
    lowStockCount: string;
    expiringMedicines: string;
    outOfStock: string;
    monthlyProfit: string;
  };
  medicines: {
    title: string;
    name: string;
    genericName: string;
    category: string;
    batchNumber: string;
    purchasePrice: string;
    sellingPrice: string;
    unitType: string;
    quantity: string;
    expiryDate: string;
    addMedicine: string;
    editMedicine: string;
    searchPlaceholder: string;
    lowStockThreshold: string;
  };
  pos: {
    title: string;
    searchMedicine: string;
    cart: string;
    total: string;
    checkout: string;
    quantity: string;
    price: string;
    subtotal: string;
    paymentMethod: string;
    cash: string;
    bank: string;
    mobile: string;
    credit: string;
    amountPaid: string;
    change: string;
    saleCompleted: string;
  };
  inventory: {
    title: string;
    medicine: string;
    batch: string;
    expiry: string;
    stock: string;
    balance: string;
    status: string;
    lowStock: string;
    outOfStock: string;
    ok: string;
    fefoOrder: string;
  };
  purchases: {
    title: string;
    newPurchase: string;
    purchaseNumber: string;
    supplier: string;
    date: string;
    totalAmount: string;
    amountPaid: string;
    amountDue: string;
    paymentStatus: string;
    addItem: string;
    savePurchase: string;
  };
  suppliers: {
    title: string;
    name: string;
    phone: string;
    email: string;
    address: string;
    addSupplier: string;
    editSupplier: string;
    paymentStatus: string;
  };
  reports: {
    title: string;
    dailySales: string;
    weeklySales: string;
    monthlySales: string;
    yearlySales: string;
    inventory: string;
    profit: string;
    supplierPurchases: string;
    staffPerformance: string;
    date: string;
    sales: string;
    profit: string;
  };
  notifications: {
    title: string;
    lowStock: string;
    outOfStock: string;
    expiryAlert: string;
    markAsRead: string;
    noNotifications: string;
  };
  settings: {
    title: string;
    pharmacyInformation: string;
    inventorySettings: string;
    databaseManagement: string;
    systemInformation: string;
    securitySettings: string;
    backupDatabase: string;
    restoreDatabase: string;
    language: string;
    currency: string;
    version: string;
    architecture: string;
    inventoryMethod: string;
    authentication: string;
    database: string;
  };
  branches: {
    title: string;
    addBranch: string;
    editBranch: string;
    name: string;
    location: string;
    phone: string;
    address: string;
    active: string;
    inactive: string;
    noBranches: string;
    createFirstBranch: string;
  };
  cosmetics: {
    title: string;
    productName: string;
    description: string;
    category: string;
    price: string;
    addCosmetic: string;
    editCosmetic: string;
    searchPlaceholder: string;
    batches: string;
  };
  users: {
    title: string;
    fullName: string;
    role: string;
    addUser: string;
    editUser: string;
    status: string;
  };
  audit: {
    title: string;
    user: string;
    action: string;
    module: string;
    date: string;
    time: string;
  };
  validation: {
    required: string;
    emailInvalid: string;
    passwordMinLength: string;
    quantityMustBePositive: string;
    expiryDateRequired: string;
  };
}

export const translations: Record<Language, Translations> = {
  en: {
    common: {
      appName: 'Milki Drug Store',
      loading: 'Loading...',
      save: 'Save',
      cancel: 'Cancel',
      delete: 'Delete',
      edit: 'Edit',
      add: 'Add',
      search: 'Search',
      actions: 'Actions',
      status: 'Status',
      active: 'Active',
      inactive: 'Inactive',
      yes: 'Yes',
      no: 'No',
      confirm: 'Confirm',
      success: 'Success',
      error: 'Error',
      warning: 'Warning',
      backToLogin: 'Back to login',
      signIn: 'Sign in',
      rememberPassword: 'Remember your password?',
    },
    auth: {
      login: 'Login',
      email: 'Email',
      password: 'Password',
      forgotPassword: 'Forgot password?',
      resetPassword: 'Reset Password',
      sendResetLink: 'Send Reset Link',
      newPassword: 'New Password',
      confirmPassword: 'Confirm Password',
      passwordChanged: 'Password changed successfully',
      passwordResetSent: 'If an account with that email exists, a reset link has been sent.',
      invalidToken: 'Invalid or missing reset token.',
      logout: 'Logout',
    },
    dashboard: {
      title: 'Dashboard',
      totalMedicines: 'Total Medicines',
      totalInventoryValue: 'Inventory Value',
      todaySales: "Today's Sales",
      monthlySales: 'Monthly Sales',
      lowStockCount: 'Low Stock',
      expiringMedicines: 'Expiring Soon',
      outOfStock: 'Out of Stock',
      monthlyProfit: 'Monthly Profit',
    },
    medicines: {
      title: 'Medicines',
      name: 'Medicine Name',
      genericName: 'Generic Name',
      category: 'Category',
      batchNumber: 'Batch Number',
      purchasePrice: 'Purchase Price',
      sellingPrice: 'Selling Price',
      unitType: 'Unit Type',
      quantity: 'Quantity',
      expiryDate: 'Expiry Date',
      addMedicine: 'Add Medicine',
      editMedicine: 'Edit Medicine',
      searchPlaceholder: 'Search medicines...',
      lowStockThreshold: 'Low Stock Threshold',
    },
    pos: {
      title: 'POS',
      searchMedicine: 'Search Medicine',
      cart: 'Cart',
      total: 'Total',
      checkout: 'Checkout',
      quantity: 'Qty',
      price: 'Price',
      subtotal: 'Subtotal',
      paymentMethod: 'Payment Method',
      cash: 'Cash',
      bank: 'Bank',
      mobile: 'Mobile',
      credit: 'Credit',
      amountPaid: 'Amount Paid',
      change: 'Change',
      saleCompleted: 'Sale completed successfully',
    },
    inventory: {
      title: 'Inventory',
      medicine: 'Medicine',
      batch: 'Batch',
      expiry: 'Expiry',
      stock: 'Stock',
      balance: 'Balance',
      status: 'Status',
      lowStock: 'Low Stock',
      outOfStock: 'Out of Stock',
      ok: 'OK',
      fefoOrder: 'FEFO Order',
    },
    purchases: {
      title: 'Purchases',
      newPurchase: 'New Purchase',
      purchaseNumber: 'Purchase Number',
      supplier: 'Supplier',
      date: 'Date',
      totalAmount: 'Total Amount',
      amountPaid: 'Amount Paid',
      amountDue: 'Amount Due',
      paymentStatus: 'Payment Status',
      addItem: 'Add Item',
      savePurchase: 'Save Purchase',
    },
    suppliers: {
      title: 'Suppliers',
      name: 'Name',
      phone: 'Phone',
      email: 'Email',
      address: 'Address',
      addSupplier: 'Add Supplier',
      editSupplier: 'Edit Supplier',
      paymentStatus: 'Payment Status',
    },
    reports: {
      title: 'Reports',
      dailySales: 'Daily Sales',
      weeklySales: 'Weekly Sales',
      monthlySales: 'Monthly Sales',
      yearlySales: 'Yearly Sales',
      inventory: 'Inventory',
      profit: 'Profit',
      supplierPurchases: 'Supplier Purchases',
      staffPerformance: 'Staff Performance',
      date: 'Date',
      sales: 'Sales',
      profit: 'Profit',
    },
    notifications: {
      title: 'Notifications',
      lowStock: 'Low Stock',
      outOfStock: 'Out of Stock',
      expiryAlert: 'Expiry Alert',
      markAsRead: 'Mark as read',
      noNotifications: 'No notifications',
    },
    settings: {
      title: 'Settings',
      pharmacyInformation: 'Pharmacy Information',
      inventorySettings: 'Inventory Settings',
      databaseManagement: 'Database Management',
      systemInformation: 'System Information',
      securitySettings: 'Security Settings',
      backupDatabase: 'Backup Database',
      restoreDatabase: 'Restore Database',
      language: 'Language',
      currency: 'Currency',
      version: 'Version',
      architecture: 'Architecture',
      inventoryMethod: 'Inventory Method',
      authentication: 'Authentication',
      database: 'Database',
    },
    branches: {
      title: 'Branch Management',
      addBranch: 'Add Branch',
      editBranch: 'Edit Branch',
      name: 'Branch Name',
      location: 'Location',
      phone: 'Phone',
      address: 'Address',
      active: 'Active',
      inactive: 'Inactive',
      noBranches: 'No branches found',
      createFirstBranch: 'Create your first branch to get started',
    },
    cosmetics: {
      title: 'Cosmetics',
      productName: 'Product Name',
      description: 'Use Description',
      category: 'Category',
      price: 'Price',
      addCosmetic: 'Add Cosmetic',
      editCosmetic: 'Edit Cosmetic',
      searchPlaceholder: 'Search cosmetics...',
      batches: 'Batches',
    },
    users: {
      title: 'Users',
      fullName: 'Full Name',
      role: 'Role',
      addUser: 'Add User',
      editUser: 'Edit User',
      status: 'Status',
    },
    audit: {
      title: 'Audit Logs',
      user: 'User',
      action: 'Action',
      module: 'Module',
      date: 'Date',
      time: 'Time',
    },
    validation: {
      required: 'This field is required',
      emailInvalid: 'Invalid email address',
      passwordMinLength: 'Password must be at least 6 characters',
      quantityMustBePositive: 'Quantity must be a positive number',
      expiryDateRequired: 'Expiry date is required',
    },
  },
  am: {
    common: {
      appName: 'ሚልኪ ድራግ ስቶር',
      loading: 'በመጫን ላይ...',
      save: 'አስቀምጥ',
      cancel: 'ሰርዝ',
      delete: 'ሰርዝ',
      edit: 'አርትዕ',
      add: 'አክል',
      search: 'ፈልግ',
      actions: 'ድርጊቶች',
      status: 'ሁኔታ',
      active: 'ንቁ',
      inactive: 'አልተገበያየ',
      yes: 'አዎ',
      no: 'አይ',
      confirm: 'አረጋግጥ',
      success: 'ተሳክቷል',
      error: 'ስህተት',
      warning: 'ማሳሰቢያ',
      backToLogin: 'ወደ መግቢያ ተመለስ',
      signIn: 'ግባ',
      rememberPassword: 'የይለፍ ቃልዎን ያስታውሳሉ?',
    },
    auth: {
      login: 'መግቢያ',
      email: 'ኢሜይል',
      password: 'ይለፍ ቃል',
      forgotPassword: 'ይለፍ ቃል ረሳች?',
      resetPassword: 'ይለፍ ቃል አድስ',
      sendResetLink: 'የማድስ ሊንክ ላክ',
      newPassword: 'አዲስ ይለፍ ቃል',
      confirmPassword: 'ይለፍ ቃል አረጋግጥ',
      passwordChanged: 'ይለፍ ቃል በተሳካ ሁኔታ ተቀይሯል',
      passwordResetSent: 'ከዚህ ኢሜይል ጋር መለያ ካለ የማድስ ሊንክ ተልኳል።',
      invalidToken: 'ልክ ያልሆነ ወይም የለለ ማድሳዊ ማስመሪያ።',
      logout: 'መውጫ',
    },
    dashboard: {
      title: 'ዳሽቦርድ',
      totalMedicines: 'ጠቅላላ መድሃኒቶች',
      totalInventoryValue: 'የማሰራጫ ዋጋ',
      todaySales: 'የዛሬ ሽያጭ',
      monthlySales: 'የወር ሽያጭ',
      lowStockCount: 'ዝቅተኛ ክምችት',
      expiringMedicines: 'ሊያበቃ የሆኑ',
      outOfStock: 'ከረር',
      monthlyProfit: 'የወር ትርፍ',
    },
    medicines: {
      title: 'መድሃኒቶች',
      name: 'የመድሃኒት ስም',
      genericName: 'ጄኔሪክ ስም',
      category: 'ምድብ',
      batchNumber: 'ባትች ቁጥር',
      purchasePrice: 'የገዢ ዋጋ',
      sellingPrice: 'የሽያጭ ዋጋ',
      unitType: 'የመለኪያ አይነት',
      quantity: 'መጠን',
      expiryDate: 'የማብቂያ ቀን',
      addMedicine: 'መድሃኒት አክል',
      editMedicine: 'መድሃኒት አርትዕ',
      searchPlaceholder: 'መድሃኒት ፈልግ...',
      lowStockThreshold: 'ዝቅተኛ ክምችት መስፈርት',
    },
    pos: {
      title: 'መሸጫ',
      searchMedicine: 'መድሃኒት ፈልግ',
      cart: 'መንቀሳቃሽ',
      total: 'ጠቅላላ',
      checkout: 'ክፈል',
      quantity: 'ብዛት',
      price: 'ዋጋ',
      subtotal: 'ንዑስ ጠቅላላ',
      paymentMethod: 'የክፍያ ዘዴ',
      cash: 'ጥሬ ገንዘብ',
      bank: 'ባንክ',
      mobile: 'ሞባይል',
      credit: 'ክሬዲት',
      amountPaid: 'የተከፈለ መጠን',
      change: 'ቀሪ',
      saleCompleted: 'ሽያጭ ተሳክቷል',
    },
    inventory: {
      title: 'ማሰራጫ',
      medicine: 'መድሃኒት',
      batch: 'ባትች',
      expiry: 'ማብቂያ',
      stock: 'ክምችት',
      balance: 'ቀሪ',
      status: 'ሁኔታ',
      lowStock: 'ዝቅተኛ ክምችት',
      outOfStock: 'ከረር',
      ok: 'በጣም ጥሩ',
      fefoOrder: 'FEFO ቅደምተና',
    },
    purchases: {
      title: 'ገዥዎች',
      newPurchase: 'አዲስ ገዥ',
      purchaseNumber: 'የገዥ ቁጥር',
      supplier: 'አቅራቢ',
      date: 'ቀን',
      totalAmount: 'ጠቅላላ መጠን',
      amountPaid: 'የተከፈለ',
      amountDue: 'የሚከፈል',
      paymentStatus: 'የክፍያ ሁኔታ',
      addItem: 'እቃ አክል',
      savePurchase: 'ገዥ አስቀምጥ',
    },
    suppliers: {
      title: 'አቅራቢዎች',
      name: 'ስም',
      phone: 'ስልክ',
      email: 'ኢሜይል',
      address: 'አድራሻ',
      addSupplier: 'አቅራቢ አክል',
      editSupplier: 'አቅራቢ አርትዕ',
      paymentStatus: 'የክፍያ ሁኔታ',
    },
    reports: {
      title: 'ሪፖርቶች',
      dailySales: 'የቀን ሽያጭ',
      weeklySales: 'የሳምንት ሽያጭ',
      monthlySales: 'የወር ሽያጭ',
      yearlySales: 'የአመት ሽያጭ',
      inventory: 'ማሰራጫ',
      profit: 'ትርፍ',
      supplierPurchases: 'የአቅራቢ ገዥዎች',
      staffPerformance: 'የሰራተኞች አፈፃፀሚት',
      date: 'ቀን',
      sales: 'ሽያጭ',
      profit: 'ትርፍ',
    },
    notifications: {
      title: 'ማሳሰቢያዎች',
      lowStock: 'ዝቅተኛ ክምችት',
      outOfStock: 'ከረር',
      expiryAlert: 'የማብቂያ ማሳሰቢያ',
      markAsRead: 'እንደተነበበ ምልክት አድርግ',
      noNotifications: 'ምንም ማሳሰቢያ የለም',
    },
    settings: {
      title: 'ማዋቀሪያዎች',
      pharmacyInformation: 'የፋርማሲ መረጃ',
      inventorySettings: 'የማሰራጫ ማዋቀሪያዎች',
      databaseManagement: 'የመረጃ ጎታ አስተዳደር',
      systemInformation: 'የስርአቱ መረጃ',
      securitySettings: 'የደህንነት ማዋቀሪያዎች',
      backupDatabase: 'መረጃ ጎታ ጠቅልይ',
      restoreDatabase: 'መረጃ ጎታ አስቀድም',
      language: 'ቋንቋ',
      currency: 'ምንዛሪ',
      version: 'ስሪት',
      architecture: 'አርክቴክቸር',
      inventoryMethod: 'የማሰራጫ ዘዴ',
      authentication: 'ማረጋገጫ',
      database: 'መረጃ ጎታ',
    },
    branches: {
      title: 'የጅምላ አስተዳደር',
      addBranch: 'ጅምላ አክል',
      editBranch: 'ጅምላ አርትዕ',
      name: 'የጅምላ ስም',
      location: 'አካባቢ',
      phone: 'ስልክ',
      address: 'አድራሻ',
      active: 'ንቁ',
      inactive: 'አልተገበያየ',
      noBranches: 'ምንም ጅምላ አልተገኘም',
      createFirstBranch: 'ለመጀመር የመጀመሪያ ጅምላ ይፍጠሩ',
    },
    cosmetics: {
      title: 'ሸማቾች',
      productName: 'የምርት ስም',
      description: 'መግለጫ',
      category: 'ምድብ',
      price: 'ዋጋ',
      addCosmetic: 'ሸማታ አክል',
      editCosmetic: 'ሸማታ አርትዕ',
      searchPlaceholder: 'ሸማታ ፈልግ...',
      batches: 'ባትቾች',
    },
    users: {
      title: 'ተጠቃሚዎች',
      fullName: 'ሙሉ ስም',
      role: 'ሚና',
      addUser: 'ተጠቃሚ አክል',
      editUser: 'ተጠቃሚ አርትዕ',
      status: 'ሁኔታ',
    },
    audit: {
      title: 'የኦዲት ምዘና',
      user: 'ተጠቃሚ',
      action: 'ድርጊት',
      module: 'ሞጁል',
      date: 'ቀን',
      time: 'ሰዓት',
    },
    validation: {
      required: 'ይህ ሜዳ ያስፈለጋል',
      emailInvalid: 'ልክ ያልሆነ ኢሜይል አድራሻ',
      passwordMinLength: 'ይለፍ ቃል ቢያንስ 6 ፊደሎች ሊኖሩት ይገባል',
      quantityMustBePositive: 'ብዛት ፊደል መሆን አለበት',
      expiryDateRequired: 'የማብቂያ ቀን ያስፈለጋል',
    },
  },
};