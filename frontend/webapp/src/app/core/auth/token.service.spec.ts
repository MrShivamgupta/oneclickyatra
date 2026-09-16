import { TestBed } from '@angular/core/testing';
import { TokenService } from './token.service';

function buildFakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe('TokenService', () => {
  let service: TokenService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenService);
    localStorage.clear();
  });

  it('starts with no active session', () => {
    expect(service.accessToken()).toBeNull();
    expect(service.isAccessTokenExpired()).toBe(true);
  });

  it('decodes roles and permissions from the access token on setSession', () => {
    const token = buildFakeJwt({
      sub: 'user-1',
      email: 'admin@example.com',
      full_name: 'Admin User',
      role: 'SuperAdmin',
      permission: ['booking.view', 'booking.create'],
      exp: Math.floor(Date.now() / 1000) + 3600
    });

    service.setSession(token, 'refresh-token-value');

    expect(service.roles()).toEqual(['SuperAdmin']);
    expect(service.permissions()).toEqual(['booking.view', 'booking.create']);
    expect(service.email()).toBe('admin@example.com');
    expect(service.isAccessTokenExpired()).toBe(false);
  });

  it('hasPermission is true for SuperAdmin regardless of explicit permission claims', () => {
    const token = buildFakeJwt({
      email: 'admin@example.com',
      full_name: 'Admin',
      role: 'SuperAdmin',
      exp: Math.floor(Date.now() / 1000) + 3600
    });

    service.setSession(token, 'refresh-token-value');

    expect(service.hasPermission('anything.not.granted')).toBe(true);
  });

  it('clearSession removes the stored refresh token', () => {
    const token = buildFakeJwt({ email: 'x@example.com', full_name: 'X', exp: Math.floor(Date.now() / 1000) + 3600 });
    service.setSession(token, 'refresh-token-value');

    service.clearSession();

    expect(service.accessToken()).toBeNull();
    expect(service.getStoredRefreshToken()).toBeNull();
  });
});
