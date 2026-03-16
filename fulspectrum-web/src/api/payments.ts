import { env } from "../config/env";

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

type RefreshAuthResponse = {
  accessToken: string;
  expiresAtUtc: string;
  profile: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: "Admin" | "Customer";
  };
};

const API_BASE = `${env.API_BASE_URL}/api/v1`;
const AUTH_STORAGE_KEY = "fulspectrum_auth";

function persistRefreshedAuth(auth: RefreshAuthResponse): void {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY);
  if (!raw) {
    return;
  }

  const parsed = JSON.parse(raw) as {
    accessToken?: string;
    profile?: RefreshAuthResponse["profile"];
    forbiddenMessage?: string | null;
  };

  localStorage.setItem(
    AUTH_STORAGE_KEY,
    JSON.stringify({
      ...parsed,
      accessToken: auth.accessToken,
      profile: auth.profile,
      forbiddenMessage: null,
    })
  );
}

async function refreshAccessToken(): Promise<string | null> {
  const response = await fetch(`${API_BASE}/auth/refresh`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    return null;
  }

  const auth = (await response.json()) as RefreshAuthResponse;
  persistRefreshedAuth(auth);
  return auth.accessToken;
}

async function fetchWithReauth(
  url: string,
  init: RequestInit,
  token: string
): Promise<Response> {
  const first = await fetch(url, {
    ...init,
    credentials: "include",
    headers: {
      ...(init.headers ?? {}),
      Authorization: `Bearer ${token}`,
    },
  });

  if (first.status !== 401) {
    return first;
  }

  const refreshedToken = await refreshAccessToken();
  if (!refreshedToken) {
    return first;
  }

  return fetch(url, {
    ...init,
    credentials: "include",
    headers: {
      ...(init.headers ?? {}),
      Authorization: `Bearer ${refreshedToken}`,
    },
  });
}

export async function createPaymentAttempt(
  orderId: string,
  body: PaymentAttemptRequest,
  idempotencyKey: string,
  token: string,
): Promise<PaymentAttemptResponse> {
  const response = await fetchWithReauth(
    `${API_BASE}/payments/orders/${orderId}/attempts`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Idempotency-Key": idempotencyKey,
      },
      body: JSON.stringify(body),
    },
    token,
  );

  if (!response.ok) {
    throw new Error(`No se pudo crear intento de pago (${response.status})`);
  }

  return response.json() as Promise<PaymentAttemptResponse>;
}

export async function getPaymentStatus(orderId: string, token: string): Promise<PaymentStatusResponse> {
  const response = await fetchWithReauth(
    `${API_BASE}/payments/orders/${orderId}/status`,
    {
      method: "GET",
    },
    token,
  );

  if (!response.ok) {
    throw new Error(`No se pudo obtener estado de pago (${response.status})`);
  }

  return response.json() as Promise<PaymentStatusResponse>;
}