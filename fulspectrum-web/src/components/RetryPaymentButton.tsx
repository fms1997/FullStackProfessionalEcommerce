import { useState } from "react";
import { createPaymentAttempt } from "../api/payments";
import { getOrCreateIdempotencyKey } from "../lib/idempotency";

type RetryPaymentButtonProps = {
  orderId: string;
  token: string;
  provider: "Stripe" | "MercadoPago";
  returnUrl?: string;
};

export function RetryPaymentButton({ orderId, token, provider, returnUrl }: RetryPaymentButtonProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onRetry = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const idempotencyKey = getOrCreateIdempotencyKey(orderId);
      const attempt = await createPaymentAttempt(orderId, { provider, returnUrl }, idempotencyKey, token);
      window.location.href = attempt.checkoutUrl;
    } catch (e) {
      const message = e instanceof Error ? e.message : "No se pudo iniciar el reintento.";
      setError(message);
      setIsLoading(false);
    }
  };

  return (
    <div>
      <button type="button" onClick={onRetry} disabled={isLoading}>
        {isLoading ? "Reintentando..." : "Reintentar pago"}
      </button>
      {error ? <p role="alert">{error}</p> : null}
    </div>
  );
}
