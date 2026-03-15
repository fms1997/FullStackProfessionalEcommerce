export const IDEMPOTENCY_KEY_STORAGE_PREFIX = "checkout-idempotency";

export function getOrCreateIdempotencyKey(orderId: string): string {
  const storageKey = `${IDEMPOTENCY_KEY_STORAGE_PREFIX}:${orderId}`;
  const existing = window.sessionStorage.getItem(storageKey);

  if (existing) {
    return existing;
  }

  const key = crypto.randomUUID();
  window.sessionStorage.setItem(storageKey, key);
  return key;
}

export function clearIdempotencyKey(orderId: string): void {
  const storageKey = `${IDEMPOTENCY_KEY_STORAGE_PREFIX}:${orderId}`;
  window.sessionStorage.removeItem(storageKey);
}
