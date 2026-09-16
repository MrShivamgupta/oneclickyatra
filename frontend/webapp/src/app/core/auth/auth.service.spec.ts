import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';

function buildFakeJwt(expOffsetSeconds: number): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(
    JSON.stringify({
      email: 'admin@oneclickyatra.dev',
      full_name: 'Admin',
      role: 'SuperAdmin',
      exp: Math.floor(Date.now() / 1000) + expOffsetSeconds
    })
  );
  return `${header}.${body}.signature`;
}

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let tokenService: TokenService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('initializeSession returns false when no refresh token is stored', async () => {
    const restored = await firstValueFrom(service.initializeSession());
    expect(restored).toBeFalsy();
  });

  it('initializeSession restores access token from refresh token', async () => {
    localStorage.setItem('ocy.refreshToken', 'stored-refresh-token');
    const promise = firstValueFrom(service.initializeSession());

    const req = httpMock.expectOne((r) => r.url.includes('/auth/refresh'));
    expect(req.request.body).toEqual({ refreshToken: 'stored-refresh-token' });
    req.flush({
      success: true,
      data: {
        accessToken: buildFakeJwt(3600),
        refreshToken: 'new-refresh-token',
        email: 'admin@oneclickyatra.dev',
        fullName: 'Admin',
        roles: ['SuperAdmin']
      },
      message: 'ok',
      trackingId: 't1'
    });

    const restored = await promise;
    expect(restored).toBeTruthy();
    expect(tokenService.accessToken()).not.toBeNull();
    expect(tokenService.getStoredRefreshToken()).toBe('new-refresh-token');
  });
});
