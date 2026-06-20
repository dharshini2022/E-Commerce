# Project Overview
Multi - Vendor Ecommerce Application

---
# Tech Stack
**Functional Requirements**
1. Backend
- .NET Web API
- EFCore & FluentAPI
- ClosedXML - PDF Downloads
- Swagger for API Endpoint Checking
- NUnit Testing
- SeriLog (File based Logging)
- SMTP mail
- JWT token based Authenticatio
- bcrypt based masking of data

2. Frontned
- Angular

3. Database
- PostgreSQL
- Stored Procedure for Dashboard operations (joins)

---
# Design Patterns
- Repository Pattern
- Dependency Injection
- Service Layer Pattern
- Dependency Inversion (Interface based abstraction)
- Builder Pattern (added components to builder based WebApp at Program.cs)
- Scoped based object LifeTime

---
# Authentication & Authorization
- JWT Tokens with 7 days window for expiry
- Hashing using bcrypt
- Authorization using RBAC [Future Scope: PBAC]
---
# Roles
- Customer
- Vendor
- Admin
---

# Functional Requirement
# ROLE : Customer

//User Table
1. **Register** 
- RegisterDTO -> Email Verification -> User Table -> Email confirmation
2. **Login**
- LoginDTO (email and password) -> Authenticate -> Token Generation
3. **Update Profile Details**
- Update profile details like name, email and password -> Email Confirmation
4. **Change Password**
- Specifically change password -> Email Verification -> Email confirmation

//UserAddress Table:
5. **Add User Address**
- This is done at Register User step.
- On at Shipment when the user is adding a new address.

6. **Get All User Adresses by UserId**
- This is done at view profile stage

//Discount Table
7. **View Discounts**
- GetAll() Discounts IsActive == true

//Cart Table
8. **View Cart**
- Get Cart by UserID => Get CartItems by CartId where isInCart = true

9. **Add to Cart**
- Get ProductId, VariantId, Quantity => Check if stock of variant >= quantity => Create CartItem with reference to the CartId of the User

10. **Remove from Cart**
- Hard Delete CartItem

11. **Update quantity of Cart Item**
- if (+) check if variant qty >cartItem qty + 1
- if (-), if qty becomes zero, trigger **Remove from Cart**

//Order Table
12. **Place Order**

Phase 1: Order Creation (Optimistic)
POST /orders
    └── Validate cart items + stock (read-only check)
    └── Create ORDER (status = PendingPayment)
    └── Create ORDER_ITEMS (unit prices frozen)
    └── Reserve stock (not decrement — soft lock)
    └── Create Razorpay order via SDK
    └── Return { orderId, razorpayOrderId, amount }
    
Phase 2: Payment (Client-Side)
    └── Angular opens Razorpay checkout modal
    └── Customer pays
    └── Razorpay returns { razorpay_payment_id, razorpay_order_id, razorpay_signature }

Phase 3: Payment Verification (Server-Side)
POST /orders/{id}/verify-payment
    └── HMAC-SHA256 signature verification
    └── On success:
        └── Decrement actual stock
        └── Release stock reservation
        └── Clear cart
        └── Order status → Confirmed
        └── Fire notification
    └── On failure:
        └── Release stock reservation
        └── Order status → PaymentFailed

Phase 4: Razorpay Webhook (Safety Net)
POST /webhooks/razorpay
    └── Independent of client — handles client crashes/drops
    └── Idempotent: checks if already processed
    └── Same success/failure logic as above
1. Create Order
2. Move CartItem (isInCart = true) to OrderItem
3. Get address and store in temp
4. Check if Discount applicable
5. Make Payment
6. Make Vendor Settlement (unitprice * qty of OrderItem = Gross Amount)
6. UpdateOrderStatus
7. Reduce Stock
8. Create shippment (try creating shipment with a delay of 2 hours)
9. Push Notification

13. **View All Orders**
Get Orders => OrderItem By UserId

14. **Cancel Order**
1. Check if OrderStatus = confirmed => cancelled
2. if OrderStatus. = Initiated => cancelled => ShipmentStatus = Cancelled (shipping amount will not be refunded in this case. If cancelled at last 24 hour 20% of the total bill will not be refunded)

15. **Return Order**
1. Check if OrderStatus = Delivered
2. Return (can return whole order or only few products from the order)
3. ReturnItems with qty
4. Shipment of order get address
5. Shipment with status Inititated + 2 days for pickup
6. After 2 days, ShipmentStatus = Picked

16. **Update Order**
1. If OrderStatus = Confirmed (not initiated, Shipment not created) => Then update address? or update stock or remove an OrderItem

//Review Table
16. **Review Product**
1. Fetch ProductID, UserID, OrderID
2. Get rating, Title & Body from User
3. Create Review 
4. Add Review Images

17. **Delete Reviews**
- Hard Delete Reviews => Hard Delete the associated ReviewImage

18. **Update Review by ReviewID**

17. **View All Reviews**
Get All Reviews by UserId

//ReviewImage Table
18. **Add ReviewImage** done at Review Product

19. **Update Review Images**
20. **Delete Review Images**


//Payment Table
18. **Make Payment**
- This is done during placing order

19. **Refund Payment**
- Amount is refunded to the customer on order cancellation

20. **View Payment History By UserId**

//Products Table
21. **GetProducts** (Dynamic URL endpoints)
- Pagination
- Sort : Prize, Ratings, Newest
- Filter : Category (all categories listed as drop down menu)
(Product Images will also be retrieved)

//Returns
22. **View all Returns By UserId**

23. **Create Return**
When the order status changes to return related one (status = Requested), 
A new return with link to the OrderId will be created. => Return Items created
- Case 1 : Admin approves:
status = Approved and after 2 days, Status = Picked and CompletedAt given & IsRefunded = true.
- Case 2 : Admin rejects
status = Rejected

//Shipments Table
24. **Create shipment**
- With shipment address , status = Pending
- after 2 hours, make it initiated
- after 2 days, make it Delivered or picked based on order or ReturnID,
- case 1 : Order
Shipment created with Address. EstimatedFullfillment = Now + 2 days, Status = Pending => Order gets shipmentID => After 2 hrs (ShippedAT = Now) Status = Initiated => after 2 days = FullfilledAt becomes the Estimated Delivery. Status = Delivered

- Case 2 : Return
Shipment created with Address. EstimatedFullfillment = Now + 2 days, Status = Pending for Return and ReturnItem => Return gets shipmentID => After 2 hrs (ShippedAT = Now) Status = Initiated => after 2 days = FulfilledAt becomes the EstimatedFullfillment. Status = Picked

- Case 3 : Cancelled
if status = Initiated, it can be cancelled. Change status = Cancelled. Else not possible

//WishList Table
25. **Add to Wishlist**
- Create a new Wishlist Item record and add to wishlist

26. **Remove From WishList**
- Hard Delete by Id

# ROLE : VENDOR
//Vendor Table
7. **Register As Vendor** -> VendorRegisterDTO -> Validate GSTNumber and PANNumber.
8. **UpdateStoreDetails**  -> EmailVerification (OTP) -> Validate new details -> Update -> EmailConfirmation

//Discount
9. **Create Discounts For Vendor Products**
- Create Discount with VendorId
- If (Product Id given, discount applicable only to the product. Else discount applicable to all the Vendor's products)

//Orders Table
10. **View Order History of Vendors**
- Get ProductIds where VendorId = current vendor's Id => Get Orders where OrderItem's ProductId = vendor's all product Id

11. **View Order By ProductId**
- Get Orders where OrderItem's ProductId = vendor's specific product Id.

12. **View Active Orders of Vendors**
- Get active orders of vendor's products

//Products Table
13. **Add Products**
- Post Product => Post ProductImages => Post ProductVariant

14. **Update Product Details**
- Update Description alone

15. **Update Product Stock**

16. **Update Product Qty**

17. **Update Product Price**

18. **View Products of Vendors**
- Filter : Category, Rating
- Show revenue too

//Product Images
19. **Add Product Image**
- PostProductImage

20. **Delete Product Image by ID**
- Status = Archived

//Product Variant
22. **Add Product Variant**
- Post Product => Post Variant => Post Images

23. **Remove Product Variants**
-  IsActive = false

24. **Update Product Variant Details**
- Price Delt, StockQty, label and value

25. **View all Returns of vendor's products**
- Order (status = "Return Requested" or "Returned" or "Partially Returned" or "PartialReturnRequested") => OrderItem => VendorId

26. **View all reviews of Vendor's Products**

//Shipment Table
27. **View Shipments by VendorId** (both ordered and returned)

//Vendor Settlement
28. **View Settlements By Id**
- Filter : Product
- Filter : Status

# ROLE : ADMIN
//Vendor Table
9. **Approve Vendor**:
- GetVendors where Status = Pending
- View Vendor by Id (masked GSTNumber, PANNumber)
- Vendor Status = Approved
- Email Confirmation

10. **Suspend Vendor**
- GetVendor By Id or StoreName
- Vendor Status = Suspended
- Email Confirmation with Reason

12. **View All Vendors**
- GetAll Vendors with masked (GSTNumber and PANNumber)

//Customer Table
11. **View All Customers**
- GetAll Customers with masked (password, email and phone)

//User Table
13. **Create Admin**
- Exisiting Admin can create new admin 
- Post(User Table)

//Discount Table
14.  **Create Discount**
- If productId specified, discount created productId specified for that product alone, else ProductId is specified for all the products.

//Category Table
15. **Create new Category**
- Get category details and create new cart

//Order Table
16. **View Full Order History**
17. **View Order History by VendorId**
18. **View Order Hisotry by ProductId**

//Payment Table
19. **View Overall Payment History** 
- dynamic end point
- Sort in desc by date
- Filter by Status

20. **View Admin Revenue**
- PlatformCommission from Orders table + PlatformComissionAmount from VendorSettlement

//Products Table
21. **View All Products**
- Filter : Vendor, Catgory
- Sort : Ratings, No.of Orders, Revenue

22. **View all returns**
- Filter : ProductId, VendorID

23. **View All UnApproved Returns**
- status = requested

24. **Approve Return**
- status  = Approved or Partially Approved wrt ReturnId
- Pick Individual ReturnItem, change status = Approved or Rejected, ReturnItemRefundStatus = Refunded on Approval of the Return Item

//Reviews Table
25. **View all Reviews**
- Filter by ProductID

//Shipments Table 
26. **View all Shipments**
- Filter : Product, Vendor

27. **View All Vendor Settlements**
- Filter : VendorId, Status



# Common functions:
//Product Variants Table
20. **GetVariant**s** by ProductID = when viewed

21. **Get Product Image by Id**
- Retrieve the Product Image this is done when the Product Details is viewed.

//Notification Table
5. **EmailVerification**
- Email OTP (CustomerMail if Vendor both Customer and StoreEMail)

6. **Email Confirmation**
- Info Email (Customer Mail, if vendor only to the Store Mail)

----
# Product Table
- **Dynamic end point based product catalogue**
1. Pagination
2. Filters : Category
3. Sort : Price, Review, Date

- Search 
1. **Search by name (Contains)**

----
# Non - Functional Requirements
- Logging : SeriLog
- Push Notification - SignalR
- Rate Limiting
- JWT authentication
- Role based access control
- Exception Filters




