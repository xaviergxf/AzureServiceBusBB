resource "azurerm_resource_group" "mainRg" {
  name     = var.resourceGroupName
  location = var.resourceGroupLocation
}

resource "azurerm_servicebus_namespace" "sb" {
  name                = "xkl-asb-bb"
  location            = var.resourceGroupLocation
  resource_group_name = azurerm_resource_group.mainRg.name
  sku                 = "Standard"

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_servicebus_namespace_authorization_rule" "sb-rule" {
  name         = "sb-rule"
  namespace_id = azurerm_servicebus_namespace.sb.id

  listen = true
  send   = true
  manage = false
}

//Order Placed Topic and Subscription
resource "azurerm_servicebus_topic" "sb-topic-order-placed" {
  name         = "order-placed"
  namespace_id = azurerm_servicebus_namespace.sb.id

  support_ordering             = true
  requires_duplicate_detection = true
}

//Billing subscription for Credit Card
resource "azurerm_servicebus_subscription" "sb-topic-order-placed-sub-billing-cc" {
  name               = "billing-api-cc"
  topic_id           = azurerm_servicebus_topic.sb-topic-order-placed.id
  max_delivery_count = 10  
}

resource "azurerm_servicebus_subscription_rule" "sb-topic-order-placed-sub-rule-billing-cc" {
  name            = "billing-api-cc-rule"
  subscription_id = azurerm_servicebus_subscription.sb-topic-order-placed-sub-billing-cc.id
  filter_type     = "SqlFilter"
  sql_filter      = "paymentType = 'credit-card'"
}

//Billing subscription for Bitcoin
resource "azurerm_servicebus_subscription" "sb-topic-order-placed-sub-billing-btc" {
  name               = "billing-api-btc"
  topic_id           = azurerm_servicebus_topic.sb-topic-order-placed.id
  max_delivery_count = 10  
}

resource "azurerm_servicebus_subscription_rule" "sb-topic-order-placed-sub-rule-billing-btc" {
  name            = "billing-api-btc-rule"
  subscription_id = azurerm_servicebus_subscription.sb-topic-order-placed-sub-billing-btc.id
  filter_type     = "SqlFilter"
  sql_filter      = "paymentType = 'btc'"
}

//Order Paid Topic and Subscription
resource "azurerm_servicebus_topic" "sb-topic-order-paid" {
  name         = "order-paid"
  namespace_id = azurerm_servicebus_namespace.sb.id

  support_ordering             = true
  requires_duplicate_detection = true
}

resource "azurerm_servicebus_subscription" "sb-topic-order-paid-sub" {
  name               = "warehouse-api"
  topic_id           = azurerm_servicebus_topic.sb-topic-order-paid.id
  max_delivery_count = 10
}

//Order Shipped Topic
resource "azurerm_servicebus_topic" "sb-topic-order-shipped" {
  name         = "order-shipped"
  namespace_id = azurerm_servicebus_namespace.sb.id

  support_ordering             = true
  requires_duplicate_detection = true
}
