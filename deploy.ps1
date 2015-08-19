$destination = "C:\prog\git\unstopables\sandbox\Website"
$configName = "Unstopables.ProductsSearch.config"

Copy-Item .\Score.ContentSearch.Algolia\bin\Debug\Score.ContentSearch.Algolia.*  $destination\bin\  -verbose
Copy-Item .\Score.ContentSearch.Algolia.Tests\SampleIndex.config  $destination\App_Config\Include\$configName -verbose