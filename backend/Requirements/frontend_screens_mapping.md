# Frontend Screens Mapping & Categorization

Based on a detailed analysis of the backend codebase—specifically the API controllers (`Ecommerce.API/Controllers`), database entities (`Ecommerce.Models`), and technical functional specifications—here is the complete map of required frontend screens.

The frontend is divided into four main areas:
1. **Common Screens (Public/Shared)**
2. **Customer Screens (Authenticated Customer Role)**
3. **Vendor Screens (Authenticated Vendor Role)**
4. **Admin Screens (Authenticated Admin Role)**

---

## 1. Common Screens (Public / Shared)
These screens do not require authentication or are shared utility screens like login/registration.

| Screen | Core Purpose / Backend Integrations | Key UI Features |
| :--- | :--- | :--- |
| **Landing & Homepage** | Shows promotional discounts, categories, and featured products. Integrates: `DiscountController.GetActiveDiscounts`, `CategoryController.GetCategoryTree`, `ProductController.GetCatalog`. | Hero banner carousel, grid of product categories, top-rated product cards, list of active discount promo announcements. |
| **Product Catalog & Search** | Allows users to browse all products, filter, search, and sort. Integrates: `ProductController.GetCatalog`, `ProductController.Search`. | Left-hand sidebar filters (categories, price range, ratings), sorting drop-downs (Price Low-to-High, Price High-to-Low, Top Reviews, Newest), pagination controls, search bar. |
| **Product Details View** | Displays detailed product info, variants, images, stock levels, and user reviews. Integrates: `ProductController.GetById`, `ProductVariantController.GetVariantById`, `ReviewController.GetProductReviews`. | Image thumbnail gallery with zoom, dynamic pricing (updates based on selected variant delta), variant dropdown/buttons (size, color), stock availability status, average rating stars, paginated list of reviews with images. |
| **User Registration** | Accounts registration. Integrates: `AuthController.Register`. | Sign-up form (Email, Full Name, Password, Confirm Password), prompt for optional address details. |
| **User Login** | Session authentication. Integrates: `AuthController.Login`. | Credential forms (Email, Password), links to Register, "Remember Me" toggle. |
| **OTP Verification Screen** | Email validation during registration, password changes, or vendor signups. Integrates: `UserController.ChangePassword`, `VendorController.CreateVendor`. | 6-digit OTP input boxes, countdown timer for resending OTP. |

---

## 2. Screens of Customers
These screens require the user to be authenticated with the **Customer** role.

| Screen | Core Purpose / Backend Integrations | Key UI Features |
| :--- | :--- | :--- |
| **Customer Dashboard / Profile** | View and update profile information. Integrates: `UserController.GetProfile`, `UserController.UpdateProfile`, `UserController.ToggleAccountStatus`. | User profile card, edit button, Account Status toggle (deactivate/activate), navigation sidebar for profile sections. |
| **Change Password** | Authenticated password updates. Integrates: `UserController.ChangePassword`. | Forms: Old Password, New Password, Confirm New Password. |
| **Address Book Management** | Manage billing and shipping addresses. Integrates: `UserController.GetAllUserAddress`, `UserController.AddUserAddress`. | Grid of address cards (Labels: "Home", "Office"), "Add Address" modal, delete and set-as-default buttons. |
| **Shopping Cart** | Manage items intended for purchase. Integrates: `CartController.GetCart`, `CartController.UpdateCartItemQuantity`, `CartController.RemoveFromCart`, `DiscountController.EvaluateCartDiscounts`. | Table of cart items with image, quantity adjuster (+/- triggers, removes item on zero), item subtotal, checkout cart totals summary card, promo code input field to evaluate discounts in real time. |
| **Checkout Portal** | Finalize order items, shipping details, and pay. Integrates: `OrderController.CreateOrder`, `PaymentController.MakePayment`. | Step-by-step wizard: (1) Select Shipping Address, (2) Confirm Order Items and Discounts, (3) Payment Gateway Integration (Stripe inputs / redirects), receipt success screen. |
| **Order History Ledger** | List all historical purchases. Integrates: `OrderController.GetMyOrders`. | Table showing Order ID, Date, Total Amount, Current Status (`Confirmed`, `Shipped`, `Delivered`, `Cancelled`), action button to view details. |
| **Order Details & Tracking** | Itemized detail of a single order and its shipment path. Integrates: `OrderController.GetOrderDetail`, `ShipmentController.GetShipmentDetails`. | Ordered items grid, address summary, payment status indicator, Shipment progress bar (Pending -> Shipped -> Out for Delivery -> Delivered), estimated delivery dates, Cancel Order button (active under refund rule constraints). |
| **Order Returns Portal** | Request full or partial returns for delivered products. Integrates: `Return` and `ReturnItems` workflows. | Select items and quantities from delivered order, refund calculation indicator, reason selection dropdown, return submission form. |
| **Wishlist Manager** | View favorited products. Integrates: `WishlistController.GetWishList`, `WishlistController.RemoveFromWishList`. | List of favorited products, "Move to Cart" button, "Remove" button. |
| **Review Writer / Editor** | Post reviews and upload product images. Integrates: `ReviewController.PostReview`, `ReviewController.PutReview`, `ReviewController.DeleteReview`. | 5-star rating selector, review title input, review description box, image upload drag-and-drop container. |
| **Vendor Registration Form** | Apply to sell on the platform. Integrates: `VendorController.CreateVendor`. | Application form: Store Name, Store Email, GSTNumber, PANNumber, Store Description, Store Logo upload. |

---

## 3. Screens of Vendors
These screens require the user to be authenticated with the **Vendor** role and have an approved store.

| Screen | Core Purpose / Backend Integrations | Key UI Features |
| :--- | :--- | :--- |
| **Vendor Dashboard** | At-a-glance store performance metrics. Integrates: `VendorController.GetMyVendorProfile`, `ReviewController.GetOverallReviews`. | Sales charts (weekly/monthly revenue), counts of active products, pending settlements counter, recent orders list, Toggle Store Status button (opens/closes store). |
| **Store Profile Manager** | Edit public vendor store metadata. Integrates: `VendorController.UpdateVendor`. | Store Name, Store Email, Description, Logo URL uploader. Displays non-editable approved GST/PAN details. |
| **Product Inventory List** | Lists all products owned by the vendor store. Integrates: `ProductController.GetMyProducts`. | Paginated table: Product Name, Category, Status (`Draft`, `Active`, `Archived`), publishing toggle (`ProductController.PublishProduct`), delete/archive button. |
| **Add/Edit Product Wizard** | Add new products or update descriptions. Integrates: `ProductController.CreateProduct`, `ProductController.UpdateProduct`. | Form: Product Title, Category Selector, Description, Publish Now checkbox. |
| **Variant & Image Workspace** | Add and update product models, sizes, colors, and stock. Integrates: `ProductVariantController` (Add, Update, Archive, AddImage, DeleteImage). | Table of product variants (label/value, SKU, stock quantity, price delta, default variant radio button). Image management pane per variant. |
| **Vendor Order Tracker** | View orders containing items from the vendor store. Integrates: `OrderController.GetVendorOrders`. | List of customer orders, itemized vendor products requested, delivery requirements, total earnings from order. |
| **Vendor Shipment Console** | Track shipment status of products ordered from the vendor. Integrates: `ShipmentController.GetVendorShipments`. | Filterable table showing Shipments, estimated delivery, status details (`Pending`, `Initiated`, `Delivered`), tracking numbers. |
| **Vendor Coupon Console** | Create, view, and disable store-specific coupons. Integrates: `DiscountController.GetMyVendorDiscounts`, `DiscountController.CreateDiscount`, `DiscountController.DeactivateDiscount`. | Coupon code generator, discount type selector (percent/flat), discount value input, usage limit and expiration controls, active coupon grid. |
| **Store Returns Monitor** | Monitor customer returns for vendor products. Integrates: Returns workflows. | Returned product list, return status indicators, customer reasons, pickup shipment details. |
| **Vendor Payout Ledger** | Log of settlements and platform deductions. Integrates: `VendorSettlementController.GetMySettlements`. | Table showing Settlement ID, Order ID, Gross Sale Amount, Platform Commission (20%), Net Payout Amount, Payout Status (`Pending`, `Paid`, `Failed`), date of settlement. |
| **Store Reviews Feedback** | Customer reviews feed for store products. Integrates: `ReviewController.GetOverallReviews` (Vendor role path). | List of customer ratings and textual reviews specific to vendor items, with links to product variant details. |

---

## 4. Screens of Admins
These screens require the user to be authenticated with the **Admin** role.

| Screen | Core Purpose / Backend Integrations | Key UI Features |
| :--- | :--- | :--- |
| **Admin Dashboard** | High-level overview of platform health and finances. Integrates: Platform statistics controllers. | Total system sales charts, administrative commission ledger (20% platform cut details), active user and vendor counts, notification center for pending actions. |
| **User Administration Console** | Manage user accounts, permissions, and roles. Integrates: `UserController.ListUsers`, `UserController.GetUserDetails`, `UserController.RevokeAdmin`. | Searchable/paginated list of platform users, profile drill-down, user suspension/deactivation controls, "Grant/Revoke Admin" buttons. |
| **Vendor Moderation & Approval** | Approve/suspend vendor applications. Integrates: `VendorController.GetAllVendors`, `VendorController.ApproveVendor`, `VendorController.CancelVendor`. | Split-view lists: (1) Pending Applications, (2) Active/Suspended Vendors. Approval form displaying masked GST/PAN numbers. Suspended with reason dialog box. |
| **Category Tree Designer** | Create and manage hierarchical categories. Integrates: `CategoryController` (Create, Update, Delete). | Interactive tree view showing parent-child category relationships, inline buttons for "Add Child Category", "Edit Name", and "Delete Category" (fails if category contains products). |
| **Global Orders & Shipments Hub** | Oversee all platform logistics. Integrates: `OrderController.GetAllOrders`, `ShipmentController.GetAllShipments`. | Advanced multi-filter tables (filter by vendor, customer, product, date, or status), download order logs. |
| **Return Approvals Queue** | Handle customer return claims and execute refunds. Integrates: Returns approval backend services. | List of return claims, detail modal highlighting ReturnItems, checkboxes to Approve or Reject individual claim items, total refund trigger button. |
| **Platform Commission Audit Ledger** | Audit payment processing. Integrates: `PaymentController.GetOverallPaymentHistory`. | Global transaction logs, payment statuses (`Paid`, `Failed`, `Refunded`), payment methods list, Transaction IDs. |
| **Platform Settlements Console** | Review payouts scheduled for vendors. Integrates: `VendorSettlementController.GetOverallSettlement`. | List of vendor payouts, filter by Vendor ID, payout status, button to execute/verify bank transfers. |
| **Global Coupon & Discount Hub** | Manage platform-wide coupons and campaigns. Integrates: `DiscountController.GetAllDiscounts`, `DiscountController.CreateDiscount`, `DiscountController.DeactivateDiscount`. | Form to create global percentage or flat discounts, disable active coupons, usage statistics metrics. |
| **Platform Reviews Moderation Feed** | Global monitoring of reviews and ratings. Integrates: `ReviewController.GetOverallReviews` (Admin path). | Feed of all reviews posted, search by product or keyword, delete spam/inappropriate review entries. |
