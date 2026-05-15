import { z } from 'zod';

export const loginSchema = z.object({
    email: z.string().email('Неверный email'),
    password: z.string().min(6, 'Минимум 6 символов'),
});

export const registerSchema = z.object({
    email: z.string().email('Неверный формат почты'),
    password: z.string().min(6, 'Пароль от 6 символов'),
    confirmPassword: z.string(),
    name: z.string().min(2, 'Имя слишком короткое'),
    cityId: z.string().min(1, 'Выберите город'),
    districtId: z.string().optional(),
}).refine((data) => data.password === data.confirmPassword, {
    message: "Пароли не совпадают",
    path: ["confirmPassword"],
});