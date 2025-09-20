import { Injectable } from '@angular/core';

const TOKEN_KEY = 'qm.jwt';
const USER_KEY = 'qm.user';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  setToken(token: string) { localStorage.setItem(TOKEN_KEY, token); }
  getToken(): string | null { return localStorage.getItem(TOKEN_KEY); }
  setUser(user: unknown) { localStorage.setItem(USER_KEY, JSON.stringify(user)); }
  getUser<T = any>(): T | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) as T : null;
  }
  clear() { localStorage.removeItem(TOKEN_KEY); localStorage.removeItem(USER_KEY); }
}
