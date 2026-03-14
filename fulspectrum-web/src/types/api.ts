export type PingResponse = {
  ok: boolean;
  ts: string;
};

export type ProductDto = {
  id: string;
  categoryId: string;
  name: string;
  slug: string;
  sku: string;
  basePrice: number;
  currency: string;
  isPublished: boolean;
  createdAtUtc: string;
};

export type PagedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type ProductListQuery = {
  search?: string;
  isPublished?: boolean;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
  page?: number;
  pageSize?: number;
};
export type CartItemDto = {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  maxAllowedQuantity: number;
  availableStock: number;
  lineTotal: number;
};

export type CartDto = {
  id: string;
  userId: string;
  rowVersion: string;
  items: CartItemDto[];
  totalItems: number;
  subtotal: number;
  currency: string;
};

export type ShippingAddress = {
  fullName: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  countryCode: string;
};

export type CheckoutItemSnapshot = {
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};

export type CheckoutTotals = {
  subtotal: number;
  shippingAmount: number;
  taxAmount: number;
  total: number;
  currency: string;
};

export type CheckoutPreview = {
  cartId: string;
  items: CheckoutItemSnapshot[];
  totals: CheckoutTotals;
  shippingAddress: ShippingAddress;
};

export type OrderDto = {
  id: string;
  userId: string;
  status: string;
  currency: string;
  subtotal: number;
  shippingAmount: number;
  taxAmount: number;
  total: number;
  items: CheckoutItemSnapshot[];
  shippingAddress: ShippingAddress;
  createdAtUtc: string;
};