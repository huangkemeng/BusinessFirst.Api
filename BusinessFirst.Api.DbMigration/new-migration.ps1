$name = $args[0];
[bool]$name
if($name){
Add-Migration $name
}else{
Write-Host "请输出本次迁移的名称！"
}