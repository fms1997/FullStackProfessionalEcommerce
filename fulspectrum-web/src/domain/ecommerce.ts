export type EntityId = string;

export interface Category {
  id: EntityId;
  name: string;
  slug: string;
  description?: string;
  isActive: boolean;
}

export interface Product {
  id: EntityId;
  categoryId: EntityId;
  name: string;
  slug: string;
  sku: string;
  basePrice: number;
  currency: "USD" | "EUR" | "MXN";
  isPublished: boolean;
}

export interface ProductVariant {
  id: EntityId;
  productId: EntityId;
  variantSku: string;
  name: string;
  priceDelta: number;
  isDefault: boolean;
}

export interface InventoryItem {
  id: EntityId;
  variantId: EntityId;
  quantityOnHand: number;
  reservedQuantity: number;
  reorderThreshold: number;
}

export interface User {
  id: EntityId;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
}

export interface CatalogSnapshot {
  categories: Category[];
  products: Product[];
  variants: ProductVariant[];
  inventory: InventoryItem[];
}
