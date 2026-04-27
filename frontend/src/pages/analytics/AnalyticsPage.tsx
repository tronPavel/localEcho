import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import {AnalyticsWidget} from "@/widgets/analytics/ui/AnalyticsWidget.tsx";

export const AnalyticsPage = () => (
    <RoutedModal title="Статистика города">
        <AnalyticsWidget />
    </RoutedModal>
);