$ErrorActionPreference = "Stop"
Import-Module mdbc
Import-Module spe

# connect to mongodb
$collections = Connect-Mdbc . ProductDataProvider *
$products = $collections | where {$_.Name -eq "products"}
$mappings = $collections | where {$_.Name -eq "sitecoreMapping"}

# find any products that haven't been imported
$productData = Get-MdbcData -Collection $products
$mappingData = Get-MdbcData -Collection $mappings
$importProducts = $productData | where {$_.id -notin ($mappingData | %{$_.externalId})}
if ($importProducts.Count -eq 0)
{
    Write-Host "No products to import"
    Return
}

# import using SPE remoting
$scriptSession = New-ScriptSession -Username 'admin' -Password 'b' -ConnectionUri 'http://mongodataintegration.acdev'
$scriptArgs = @{
	products = $importProducts
}
$newMappings = Invoke-RemoteScript -Session $scriptSession -ArgumentList $scriptArgs -ScriptBlock {
	$mappings = @()
	foreach ($product in $params.products)
	{
		$item = New-Item -ItemType "MongoDataIntegration\BookProduct" -Name $product.id -Path "master:\sitecore\content\Products"
        $item.ExternalId = $product.id
		$mappings += New-Object PSObject -Property @{
			externalId = $product.id
			sitecoreId = $item.ID.ToShortID().ToString()
            mongoId = $product._id.ToString()
		}
	}
	return $mappings
}

# add the mappings into our mongodb collection
$newMappings | Add-MdbcData -Collection $mappings
Write-Host "$($newMappings.Count) products imported"
