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

//Billing subscription for Switzerland
resource "azurerm_servicebus_subscription" "sb-topic-order-placed-sub-billing-ch" {
  name               = "billing-api-ch"
  topic_id           = azurerm_servicebus_topic.sb-topic-order-placed.id
  max_delivery_count = 10  
}

resource "azurerm_servicebus_subscription_rule" "sb-topic-order-placed-sub-rule-billing-ch" {
  name            = "billing-api-ch-rule"
  subscription_id = azurerm_servicebus_subscription.sb-topic-order-placed-sub-billing-ch.id
  filter_type     = "SqlFilter"
  sql_filter      = "country = 'ch'"
}

//Billing subscription for Vietnam
resource "azurerm_servicebus_subscription" "sb-topic-order-placed-sub-billing-vn" {
  name               = "billing-api-vn"
  topic_id           = azurerm_servicebus_topic.sb-topic-order-placed.id
  max_delivery_count = 10  
}

resource "azurerm_servicebus_subscription_rule" "sb-topic-order-placed-sub-rule-billing-vn" {
  name            = "billing-api-ch-rule"
  subscription_id = azurerm_servicebus_subscription.sb-topic-order-placed-sub-billing-vn.id
  filter_type     = "SqlFilter"
  sql_filter      = "country = 'vn'"
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
