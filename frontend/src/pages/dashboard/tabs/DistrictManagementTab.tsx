import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { DistrictEditor } from '@/widgets/district-editor/ui/DistrictEditor';
import { Input } from '@/shared/ui/Input/Input';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Button } from '@/shared/ui/Button/Button';
import { toast } from 'sonner';
import styles from './Tabs.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";

export const DistrictManagementTab = () => {
    const queryClient = useQueryClient();

    const [view, setView] = useState<null | 'create' | any>(null);
    const [formName, setFormName] = useState('');
    const [formDesc, setFormDesc] = useState('');

    const { data: districts = [], isLoading } = useQuery({
        queryKey: ['admin-districts'],
        queryFn: districtApi.getForMap
    });

    const mutation = useMutation({
        mutationFn: (geometry: any[]) => {
            const data = { name: formName, description: formDesc, geometry };
            if (view === 'create') return districtApi.admin.create(data);
            return districtApi.admin.update(view.id, data);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-districts'] });
            queryClient.invalidateQueries({ queryKey: ['districts-map'] });
            toast.success(view === 'create' ? "Район создан" : "Границы обновлены");
            closeEditor();
        },
        onError: (err: any) => toast.error(err.response?.data?.detail || "Ошибка ГИС-операции")
    });

    const openCreate = () => {
        setFormName('');
        setFormDesc('');
        setView('create');
    };

    const openEdit = (district: any) => {
        setFormName(district.name);
        setFormDesc(district.description || '');
        setView(district);
    };

    const closeEditor = () => setView(null);

    if (isLoading) return <div>Загрузка реестра территорий...</div>;

    return (
        <div className={styles.tabContent}>
            {!view ? (
                <>
                    <div className={styles.tabHeader}>
                        <h2>Управление районами</h2>
                        <Button onClick={openCreate}>🆕 Добавить район</Button>
                    </div>

                    <div className={styles.districtsGrid}>
                        {districts.map(d => (
                            <div key={d.id} className={styles.districtMiniCard}>
                                <h4>{d.name}</h4>
                                <p>{d.geometry.length} точек в контуре</p>
                                <button onClick={() => openEdit(d)}>📐 Изменить границы и инфо</button>
                            </div>
                        ))}
                    </div>
                </>
            ) : (
                <div className={styles.editorContainer}>
                    <div className={styles.editorHeader}>
                        <h3>{view === 'create' ? 'Создание нового района' : `Правка: ${formName}`}</h3>
                        <Button variant="outline" size="small" onClick={closeEditor}>Назад к списку</Button>
                    </div>

                    <div className={styles.formSide}>
                        <Input
                            label="Название района"
                            value={formName}
                            onChange={e => setFormName(e.target.value)}
                            placeholder="Напр: Октябрьский"
                        />
                        <Textarea
                            label="Описание"
                            value={formDesc}
                            onChange={e => setFormDesc(e.target.value)}
                            placeholder="Краткая информация о территории..."
                        />
                    </div>

                    <DistrictEditor
                        initialGeometry={view === 'create' ? [] : view.geometry}
                        excludeId={view === 'create' ? undefined : view.id} // Передаем ID для фильтрации
                        onSave={(coords) => {
                            if (!formName) return toast.error("Введите название района");
                            if (coords.length < 3) return toast.error("Нарисуйте полигон (минимум 3 точки)");
                            mutation.mutate(coords);
                        }}
                    />
                </div>
            )}
        </div>
    );
};