$ErrorActionPreference = "Stop"
import-module mdbc

Connect-Mdbc . ProductDataProvider products

$data = Get-MdbcData (New-MdbcQuery id -EQ bk101)
$data._id | Update-MdbcData (New-MdbcUpdate -Set @{author = "Wesselman, Nick"})

$data = Get-MdbcData (New-MdbcQuery id -EQ bk102)
$data._id | Update-MdbcData (New-MdbcUpdate -Set @{genre = "SciFi"})

$data = Get-MdbcData (New-MdbcQuery id -EQ bk103)
$data._id | Update-MdbcData (New-MdbcUpdate -Set @{genre = "LoremIpsum"})