import { describe, expect, it } from "vitest";
import { clearIdempotencyKey, getOrCreateIdempotencyKey } from "./idempotency";

describe("getOrCreateIdempotencyKey", () => {
  it("reutiliza la misma key para la misma orden", () => {
    const orderId = "order-1";

    const first = getOrCreateIdempotencyKey(orderId);
    const second = getOrCreateIdempotencyKey(orderId);

    expect(first).toBeTruthy();
    expect(second).toBe(first);

    clearIdempotencyKey(orderId);
  });
});
