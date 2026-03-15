export type PaymentAttemptRequest = {
  provider: "Stripe" | "MercadoPago";
  returnUrl?: string;
};

export type PaymentAttemptResponse = {
  paymentId: string;
  orderId: string;
  provider: string;
  status: string;
  checkoutUrl: string;
  externalReference: string;
  providerPaymentId?: string;
};

export type PaymentStatusResponse = {
  orderId: string;
  paymentId?: string;
  status: "NotStarted" | "Created" | "Pending" | "Authorized" | "Succeeded" | "Failed" | "Canceled" | "Refunded";
  provider: string;
  updatedAtUtc: string;
  failureMessage?: string;
};

const API_BASE = "/api/v1";

export async function createPaymentAttempt(
  orderId: string,
  body: PaymentAttemptRequest,
  idempotencyKey: string,
  token: string,
): Promise<PaymentAttemptResponse> {
  const response = await fetch(`${API_BASE}/payments/orders/${orderId}/attempts`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      "Idempotency-Key": idempotencyKey,
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(`No se pudo crear intento de pago (${response.status})`);
  }

  return response.json() as Promise<PaymentAttemptResponse>;
}

export async function getPaymentStatus(orderId: string, token: string): Promise<PaymentStatusResponse> {
  const response = await fetch(`${API_BASE}/payments/orders/${orderId}/status`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error(`No se pudo obtener estado de pago (${response.status})`);
  }

  return response.json() as Promise<PaymentStatusResponse>;
}
