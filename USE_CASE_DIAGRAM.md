# Use Case Diagram - Milki Drug Store Management System

```mermaid
useCaseDiagram
    %% Actors
    actor "Pharmacist" as Pharmacist
    actor "Admin" as Admin
    actor "System" as System

    %% Packages for grouping
    package "Authentication & Security" {
        usecase "UC-01: Login" as UC01
        usecase "UC-02: Register (Pending Approval)" as UC02
        usecase "UC-03: Change Password" as UC03
        usecase "UC-04: Forgot Password / Reset Password" as UC04
        usecase "UC-05: Token Refresh" as UC05
        usecase "UC-06: Approve User (Admin)" as UC06
    }

    package "Point of Sale (POS)" {
        usecase "UC-07: Search Products (Medicines & Cosmetics)" as UC07
        usecase "UC-08: Add to Cart (FEFO Batch Selection)" as UC08
        usecase "UC-09: Manage Cart (Qty, Discount)" as UC09
        usecase "UC-10: Apply Discount (w/ Reason)" as UC10
        usecase "UC-11: Process Sale (Payment Methods)" as UC11
        usecase "UC-12: Print/Generate Receipt" as UC12
    }

    package "Inventory Management" {
        usecase "UC-13: View Inventory (Unified Medicines & Cosmetics)" as UC13
        usecase "UC-14: Filter/Search Inventory" as UC14
        usecase "UC-15: View Batch Details (Expiry, Stock, Prices)" as UC15
        usecase "UC-16: Monitor Low Stock" as UC16
        usecase "UC-17: View Inventory Valuation (Admin)" as UC17
    }

    package "Medicine Management" {
        usecase "UC-18: Create Medicine (Admin)" as UC18
        usecase "UC-19: Update Medicine (Admin)" as UC19
        usecase "UC-20: Delete Medicine (Admin)" as UC20
        usecase "UC-21: Add Medicine Batch (Admin)" as UC21
    }

    package "Cosmetic Management" {
        usecase "UC-22: Create Cosmetic (Admin)" as UC22
        usecase "UC-23: Update Cosmetic (Admin)" as UC23
        usecase "UC-24: Delete Cosmetic (Admin)" as UC24
        usecase "UC-25: Add Cosmetic Batch (Admin)" as UC25
    }

    package "Purchase Management" {
        usecase "UC-26: View Purchases (Admin)" as UC26
        usecase "UC-27: Create Purchase Order (Admin)" as UC27
        usecase "UC-28: View Purchase Details (Admin)" as UC28
    }

    package "Supplier Management" {
        usecase "UC-29: View Suppliers (Admin)" as UC29
        usecase "UC-30: Create Supplier (Admin)" as UC30
        usecase "UC-31: Update Supplier (Admin)" as UC31
        usecase "UC-32: Delete Supplier (Admin)" as UC32
    }

    package "Damage & Expiry Management" {
        usecase "UC-33: View Expiring Soon Items (Auto-Detected)" as UC33
        usecase "UC-34: View Expired Items (Auto-Detected)" as UC34
        usecase "UC-35: Record Damage (Manual)" as UC35
        usecase "UC-36: View Damage History" as UC36
    }

    package "Reports & Analytics" {
        usecase "UC-37: Dashboard Summary (Admin)" as UC37
        usecase "UC-38: Sales Report" as UC38
        usecase "UC-39: Most Selling Report" as UC39
        usecase "UC-40: Inventory Report" as UC40
        usecase "UC-41: Profit Report" as UC41
        usecase "UC-42: Supplier Report" as UC42
        usecase "UC-43: Staff Report" as UC43
        usecase "UC-44: Export Reports" as UC44
    }

    package "Branch Management (Admin)" {
        usecase "UC-45: View Branches" as UC45
        usecase "UC-46: Create Branch" as UC46
        usecase "UC-47: Update Branch" as UC47
        usecase "UC-48: Delete Branch" as UC48
        usecase "UC-49: Switch Branch Context" as UC49
    }

    package "User Management (Admin)" {
        usecase "UC-50: View Users" as UC50
        usecase "UC-51: Create User" as UC51
        usecase "UC-52: Update User" as UC52
        usecase "UC-53: Delete User" as UC53
        usecase "UC-54: Approve User Registration" as UC54
    }

    package "Settings & Audit" {
        usecase "UC-55: Manage System Settings (Admin)" as UC55
        usecase "UC-56: View Audit Logs (Admin)" as UC56
        usecase "UC-57: View Notifications" as UC57
    }

    %% Relationships - Pharmacist
    Pharmacist --> UC01
    Pharmacist --> UC03
    Pharmacist --> UC04
    Pharmacist --> UC05
    Pharmacist --> UC07
    Pharmacist --> UC08
    Pharmacist --> UC09
    Pharmacist --> UC10
    Pharmacist --> UC11
    Pharmacist --> UC12
    Pharmacist --> UC13
    Pharmacist --> UC14
    Pharmacist --> UC15
    Pharmacist --> UC16
    Pharmacist --> UC33
    Pharmacist --> UC34
    Pharmacist --> UC35
    Pharmacist --> UC36
    Pharmacist --> UC49
    Pharmacist --> UC57

    %% Relationships - Admin (includes all Pharmacist capabilities)
    Admin --> UC01
    Admin --> UC02
    Admin --> UC03
    Admin --> UC04
    Admin --> UC05
    Admin --> UC06
    Admin --> UC07
    Admin --> UC08
    Admin --> UC09
    Admin --> UC10
    Admin --> UC11
    Admin --> UC12
    Admin --> UC13
    Admin --> UC14
    Admin --> UC15
    Admin --> UC16
    Admin --> UC17
    Admin --> UC18
    Admin --> UC19
    Admin --> UC20
    Admin --> UC21
    Admin --> UC22
    Admin --> UC23
    Admin --> UC24
    Admin --> UC25
    Admin --> UC26
    Admin --> UC27
    Admin --> UC28
    Admin --> UC29
    Admin --> UC30
    Admin --> UC31
    Admin --> UC32
    Admin --> UC33
    Admin --> UC34
    Admin --> UC35
    Admin --> UC36
    Admin --> UC37
    Admin --> UC38
    Admin --> UC39
    Admin --> UC40
    Admin --> UC41
    Admin --> UC42
    Admin --> UC43
    Admin --> UC44
    Admin --> UC45
    Admin --> UC46
    Admin --> UC47
    Admin --> UC48
    Admin --> UC49
    Admin --> UC50
    Admin --> UC51
    Admin --> UC52
    Admin --> UC53
    Admin --> UC54
    Admin --> UC55
    Admin --> UC56
    Admin --> UC57

    %% Relationships - System (Automated)
    System --> UC33
    System --> UC34
    System .> UC05 : includes
    System .> UC11 : extends
    System .> UC35 : extends
```

## Functional Requirements Summary (Section 3.2.1)

### 1. Authentication & Security
- **User Login/Register**: JWT-based authentication with role-based access control (Admin, Pharmacist)
- **Password Management**: Change password, forgot/reset password via email
- **Token Refresh**: Secure token rotation with refresh tokens
- **User Approval Workflow**: New registrations require admin approval

### 2. Point of Sale (POS)
- **Product Search**: Search medicines and cosmetics by name, generic name, batch number, category
- **FEFO Batch Selection**: Automatic First-Expired-First-Out batch selection
- **Cart Management**: Add/remove items, adjust quantities, apply item-level discounts
- **Discount Control**: Pharmacists ≤5%, Admins ≤100% with mandatory reason
- **Multi-Payment Methods**: Cash, Bank Transfer, Mobile Money, Credit
- **Receipt Generation**: Printable receipts with sale details

### 3. Inventory Management
- **Unified View**: Combined medicines and cosmetics inventory
- **Batch-Level Tracking**: Expiry dates, stock quantities, purchase/selling prices
- **Supplier Tracking**: Per-batch supplier information
- **Stock Status**: Real-time status (OK, Low, Out of Stock, Expiring, Expired)
- **Valuation**: Inventory value at purchase price and selling price (Admin only)
- **Advanced Filtering**: By category, type, stock status, search terms

### 4. Medicine Management (Admin Only)
- **CRUD Operations**: Create, read, update, delete medicines
- **Batch Management**: Add batches with expiry, pricing, supplier info
- **Categorization**: Category and unit type assignment

### 5. Cosmetic Management (Admin Only)
- **CRUD Operations**: Create, read, update, delete cosmetics
- **Batch Management**: Add batches with balance tracking
- **Category & Unit Management**: Organized by category and unit type

### 6. Purchase Management (Admin Only)
- **Purchase Orders**: Create and track supplier purchases
- **Batch Creation**: Purchases automatically create inventory batches
- **Branch-Scoped**: Purchases tied to specific branches

### 7. Supplier Management (Admin Only)
- **CRUD Operations**: Full supplier lifecycle management
- **Purchase Association**: Link suppliers to purchase orders

### 8. Damage & Expiry Management
- **Auto-Detection**: System automatically identifies expiring (≤180 days) and expired items
- **Damage Recording**: Manual recording with reason, quantity, batch selection
- **Automatic Write-Off**: Expired batches removed from available stock
- **Notification System**: 15-day interval expiry alerts

### 9. Reports & Analytics (Admin Only)
- **Dashboard Summary**: Key metrics (sales, profit, inventory, staff)
- **Sales Reports**: Period-based with bounds
- **Most Selling Products**: Top performers by quantity/revenue
- **Inventory Reports**: Stock levels, valuation, expiry analysis
- **Profit Reports**: Margins by product, category, period
- **Supplier Reports**: Purchase history, performance
- **Staff Reports**: Individual sales performance
- **Export Capability**: Download reports

### 10. Branch Management (Admin Only)
- **Multi-Branch Support**: Create, update, delete branches
- **Context Switching**: Users can switch active branch
- **Data Isolation**: Branch-scoped data access

### 11. User Management (Admin Only)
- **User CRUD**: Full user lifecycle
- **Role Assignment**: Admin/Pharmacist roles
- **Branch Assignment**: Users assigned to branches
- **Approval Workflow**: Pending user approvals

### 12. Settings & Audit
- **System Settings**: Pharmacy name, currency, notifications
- **Audit Logs**: Comprehensive activity tracking (Admin)
- **Notifications**: In-app notification center

### Cross-Cutting Concerns
- **Role-Based Access Control**: Admin vs Pharmacist permissions
- **Branch-Level Data Isolation**: Users see only their branch data
- **Audit Trail**: All critical operations logged
- **Responsive UI**: Mobile-friendly, collapsible sidebar
- **Dark/Light Theme**: User preference persistence