import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../Services/auth.service';
import type { LoginRequest } from '../Interface/login-request.interface';
import type { RegisterRequest } from '../Interface/register-request.interface';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export default class Login implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  errorMsg = signal('');

  // Forms
  loginForm = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  registerForm = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    gmail: ['', [Validators.required, Validators.email]],
    idEmpleado: [null as number | null, [Validators.required, Validators.min(1)]],
  });

  ngOnInit(): void {
    // Clase para difuminar fondo de TODA la app mientras estás en /login
    document.body.classList.add('blur-bg');
  }

  ngOnDestroy(): void {
    document.body.classList.remove('blur-bg');
  }

  submitLogin() {
    this.errorMsg.set('');
    if (this.loginForm.invalid) { this.loginForm.markAllAsTouched(); return; }

    this.loading.set(true);
    this.auth.login(this.loginForm.value as LoginRequest).subscribe({
      next: (res) => {
        this.auth.setToken(res.token);
        this.loading.set(false);
        this.router.navigateByUrl('/home');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(this.mapError(err));
      }
    });
  }

  submitRegister() {
    this.errorMsg.set('');
    if (this.registerForm.invalid) { this.registerForm.markAllAsTouched(); return; }

    const payload = this.registerForm.value as RegisterRequest;

    // regla @gmail.com (tu API lo valida; aquí damos feedback inmediato)
    if (!payload.gmail.toLowerCase().endsWith('@gmail.com')) {
      this.errorMsg.set('El correo debe ser @gmail.com');
      return;
    }

    this.loading.set(true);
    this.auth.register(payload).subscribe({
      next: () => {
        // Registrado: ahora logueamos automáticamente
        const creds: LoginRequest = {
          username: payload.username,
          password: payload.password
        };
        this.auth.login(creds).subscribe({
          next: (res) => {
            this.auth.setToken(res.token);
            this.loading.set(false);
            this.router.navigateByUrl('/home');
          },
          error: (err) => {
            this.loading.set(false);
            this.errorMsg.set(this.mapError(err));
          }
        });
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMsg.set(this.mapError(err));
      }
    });
  }

  private mapError(err: any): string {
    if (err?.status === 401) return 'Credenciales inválidas.';
    if (err?.status === 409) return err?.error || 'El usuario o correo ya existe.';
    if (err?.status === 400) return err?.error || 'Datos inválidos.';
    if (err?.status === 0) return 'No se pudo conectar con el servidor.';
    return err?.error?.title || err?.error?.message || err?.message || 'Error inesperado.';
  }
}
