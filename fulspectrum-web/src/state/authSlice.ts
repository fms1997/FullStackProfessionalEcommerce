import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export type UserProfile = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: "Admin" | "Customer";
};

type AuthState = {
  accessToken: string | null;
  profile: UserProfile | null;
  forbiddenMessage: string | null;
};

const storageKey = "fulspectrum_auth";

const loadInitialState = (): AuthState => {
  try {
    const raw = localStorage.getItem(storageKey);
    if (!raw) return { accessToken: null, profile: null, forbiddenMessage: null };
    const parsed = JSON.parse(raw) as AuthState;
    return {
      accessToken: parsed.accessToken,
      profile: parsed.profile,
      forbiddenMessage: null,
    };
  } catch {
    return { accessToken: null, profile: null, forbiddenMessage: null };
  }
};

const persist = (state: AuthState) => {
  localStorage.setItem(
    storageKey,
    JSON.stringify({ accessToken: state.accessToken, profile: state.profile, forbiddenMessage: null }),
  );
};

const initialState = loadInitialState();

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials(state, action: PayloadAction<{ accessToken: string; profile: UserProfile }>) {
      state.accessToken = action.payload.accessToken;
      state.profile = action.payload.profile;
      state.forbiddenMessage = null;
      persist(state);
    },
    clearAuth(state) {
      state.accessToken = null;
      state.profile = null;
      state.forbiddenMessage = null;
      persist(state);
    },
    setForbidden(state, action: PayloadAction<string | null>) {
      state.forbiddenMessage = action.payload;
    },
  },
});

export const { setCredentials, clearAuth, setForbidden } = authSlice.actions;
export default authSlice.reducer;
