import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useCreateMarker } from '../model/useCreateMarker';
import type { MarkerCategory } from '@/entities/marker';

import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Select } from '@/shared/ui/Select/Select';
import { ImageUploader } from '@/shared/ui/ImageUploader/ImageUploader';

import styles from './MarkerCreateForm.module.css';
import {useMapInteractionStore} from "@/features/map-control/model/interactionStore.ts";
import {toast} from "sonner";

interface FormFields {
    title: string;
    description: string;
    category: MarkerCategory;
    startDate?: string;
    endDate?: string;
}

export const MarkerCreateForm = ({ onSuccess }: { onSuccess: () => void }) => {
    const { tempPoints } = useMapInteractionStore();
    const [files, setFiles] = useState<File[]>([]);
    const { mutate, isPending } = useCreateMarker();

    const { register, handleSubmit, watch, formState: { errors } } = useForm<FormFields>({
        defaultValues: { category: 'Issue' }
    });

    const category = watch('category');

    const onSubmit = (data: FormFields) => {
        if (data.category === 'Event' && data.startDate && data.endDate) {
            if (new Date(data.endDate) < new Date(data.startDate)) {
                return toast.error("Дата окончания не может быть раньше начала");
            }
        }

        mutate({
            ...data,
            points: tempPoints,
            imageFiles: files
        }, {
            onSuccess: () => onSuccess()
        });
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
            <div className={styles.content}>
                <Input
                    label="Заголовок"
                    placeholder="Что именно произошло?"
                    {...register('title', { required: 'Введите название' })}
                    error={errors.title?.message}
                />

                <Textarea
                    label="Описание ситуации"
                    placeholder="Добавьте подробностей для соседей и ведомств..."
                    {...register('description')}
                />

                <Select label="Тип события" {...register('category')}>
                    <option value="Issue">⚠️ Проблема</option>
                    <option value="Event">🎉 Мероприятие</option>
                    <option value="Announcement">📢 Объявление</option>
                    <option value="Suggestion">💡 Предложение</option>
                </Select>

                {category === 'Event' && (
                    <div className={styles.row}>
                        <Input
                            label="Дата и время начала"
                            type="datetime-local"
                            {...register('startDate', { required: 'Укажите начало' })}
                        />
                        <Input
                            label="Дата и время окончания"
                            type="datetime-local"
                            {...register('endDate', { required: 'Укажите конец' })}
                        />
                    </div>
                )}

                <ImageUploader label="Добавить фото (ДО)" onFilesChange={setFiles} />
            </div>

            <div className={styles.footer}>
                <Button type="submit" disabled={isPending}>
                    {isPending ? 'Загрузка...' : ' Опубликовать на карте'}
                </Button>
            </div>
        </form>
    );
};