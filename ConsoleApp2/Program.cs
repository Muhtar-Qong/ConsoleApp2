using System.Data.SqlClient;
class Program
{
    static void Main()
    {
        // we need some parameters to input from the standard input
        Console.WriteLine("Enter full path of the Thumbnail directory:");
        string? thumbnailsDirPath = Console.ReadLine();

        Console.WriteLine("Enter full path of the Large Image Directory:");
        var largeImagesDirPath = Console.ReadLine();

        Console.WriteLine("Enter Product Id:");
        var productId = int.Parse(Console.ReadLine());

        var connectionString = "Server=DESKTOP-72L6ECU\\SQLEXPRESS; Database=SigmaDB; User Id=sa; Password=myPass;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            // Create table
            string createTableQuery = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE name='Products' and type='U')
                CREATE TABLE Products (
                    Id INT PRIMARY KEY IDENTITY,
                    ProductId INT,
                    Name NVARCHAR(100),
                    Price DECIMAL(18,2),
                    Thumbnail NVARCHAR(MAX),
                    ImagePath NVARCHAR(1024),
                    FavoriteCount INT,
                    CreatedDate DATETIME
                )";
            using (SqlCommand command = new SqlCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            // get all thumbnails and large images as files
            var thumbnailFiles = Directory.GetFiles(thumbnailsDirPath);
            var largeImageFiles = Directory.GetFiles(largeImagesDirPath);

            // Since we have multiple thumbnails and large images, we should use foreach() loop here
            foreach (var file in thumbnailFiles)
            {
                // first, convert the thumbnail to byte[], then convert it to Base64String object
                byte[] thumbnailBytes = File.ReadAllBytes(file);
                var thumbnailBase64 = Convert.ToBase64String(thumbnailBytes);

                // filenames should be the same in thumbnails and largeImages directories
                var largeImagePath = largeImagesDirPath + "\\" + Path.GetFileName(file);

                var product = new Product
                {
                    ProductId = productId,
                    Name = "Neckless",
                    Price = (decimal)64.00,
                    Thumbnail = thumbnailBase64,
                    ImagePath = largeImagePath,
                    FavoriteCount = 0
                };

                // in here, we need to save the product object in the database. 
                // let's create a save image data function.
                SaveImageData(connection, product);
            }

            connection.Close();
            Console.WriteLine("All images have been saved successfully!");
        }
    }

    private static void SaveImageData(SqlConnection connection, Product product)
    {
        if (product != null)
        {
            // Insert image data
            var insertQuery = "INSERT INTO Products(ProductId, Name, Price, Thumbnail, ImagePath, FavoriteCount, CreatedDate) " +
                                            "VALUES(@ProductId,@Name,@Price,@Thumbnail,@ImagePath,@FavoriteCount,@CreatedDate)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                // Insert image data
                command.Parameters.AddWithValue("@ProductId", product.ProductId);
                command.Parameters.AddWithValue("@Name", product.Name);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Thumbnail", product.Thumbnail);  // Base64String
                command.Parameters.AddWithValue("@ImagePath", product.ImagePath);  // large image path
                command.Parameters.AddWithValue("@FavoriteCount", product.FavoriteCount);
                command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }
}


public class Product
{
    public int Id { get; set; }                         // primary key of the table
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string? Thumbnail { get; set; }              // Base64String
    public string? ImagePath { get; set; }
    public int FavoriteCount { get; set; }
    public DateTime? CreatedDate { get; set; }
}