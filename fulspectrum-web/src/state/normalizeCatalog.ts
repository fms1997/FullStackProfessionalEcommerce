import type {
  CatalogSnapshot,
  Category,
  InventoryItem,
  Product,
  ProductVariant,
} from "../domain/ecommerce";

type EntityMap<T extends { id: string }> = Record<string, T>;

export interface NormalizedCatalogState {
  categories: EntityMap<Category>;
  products: EntityMap<Product>;
  variants: EntityMap<ProductVariant>;
  inventory: EntityMap<InventoryItem>;
  productIdsByCategoryId: Record<string, string[]>;
  variantIdsByProductId: Record<string, string[]>;
  inventoryIdByVariantId: Record<string, string>;
}

const toMap = <T extends { id: string }>(items: T[]): EntityMap<T> =>
  items.reduce<EntityMap<T>>((acc, item) => {
    acc[item.id] = item;
    return acc;
  }, {});

export const normalizeCatalog = (
  snapshot: CatalogSnapshot,
): NormalizedCatalogState => {
  const categories = toMap(snapshot.categories);
  const products = toMap(snapshot.products);
  const variants = toMap(snapshot.variants);
  const inventory = toMap(snapshot.inventory);

  const productIdsByCategoryId = snapshot.products.reduce<
    Record<string, string[]>
  >((acc, product) => {
    acc[product.categoryId] ??= [];
    acc[product.categoryId].push(product.id);
    return acc;
  }, {});

  const variantIdsByProductId = snapshot.variants.reduce<
    Record<string, string[]>
  >((acc, variant) => {
    acc[variant.productId] ??= [];
    acc[variant.productId].push(variant.id);
    return acc;
  }, {});

  const inventoryIdByVariantId = snapshot.inventory.reduce<
    Record<string, string>
  >((acc, item) => {
    acc[item.variantId] = item.id;
    return acc;
  }, {});

  return {
    categories,
    products,
    variants,
    inventory,
    productIdsByCategoryId,
    variantIdsByProductId,
    inventoryIdByVariantId,
  };
};
