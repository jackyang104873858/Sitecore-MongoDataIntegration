$ErrorActionPreference = "Stop"
import-module mdbc

Connect-Mdbc . ProductDataProvider products

[xml]$books = gc "C:\git\Sitecore-MongoDataIntegration\src\Sitecore.MongoDataIntegration\App_Data\books.xml"

foreach($book in $books.catalog.book)
{
    @{
        id = $book.id
        author = $book.author
        title = $book.title
        genre = $book.genre
        price = $book.price
        publishDate = $book.publish_date
        description = $book.description
        stock = $book.stock
        sku = $book.sku
        weight = $book.weight
    } | Add-MdbcData
}

Get-MdbcData