import type { TooltipContentProps } from 'recharts';

type RechartsValueType = number | string | ReadonlyArray<number | string>;
type RechartsNameType = number | string;

export type RechartsTooltipProps = Partial<TooltipContentProps<RechartsValueType, RechartsNameType>>;
export type RechartsTooltipPayloadItem = NonNullable<RechartsTooltipProps['payload']>[number];
