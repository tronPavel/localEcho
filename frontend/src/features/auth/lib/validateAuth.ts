import { z } from 'zod';

export const loginSchema = z.object({
    email: z.string().email('Неверный email'),
    password: z.string().min(6, 'Минимум 6 символов'),
});

export const registerSchema = z.object({
    email: z.string().email('Неверный email'),
    password: z.string().min(6, 'Минимум 6 символов'),
    confirmPassword: z.string(),
    name: z.string().min(2, 'Минимум 2 символа'),
    districtId: z.string().min(1, 'Выберите район'),
    homeAddress: z.string().optional(),
}).refine((data) => data.password === data.confirmPassword, {
    message: 'Пароли не совпадают',
    path: ['confirmPassword'],
});