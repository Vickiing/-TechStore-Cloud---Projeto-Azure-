IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Description nvarchar(1000) NOT NULL CONSTRAINT DF_Products_Description DEFAULT (N''),
        Price decimal(18,2) NOT NULL,
        StockQuantity int NOT NULL,
        CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
    );
END;
