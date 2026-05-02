/**
 * Design system (Atomic Design + SCSS modules).
 * Barril público: `import { Button, Input } from "@/components/ui"`.
 * Dentro de `components/ui/*`, importar entre átomos/moléculas por caminho relativo (evita ciclo com este index).
 */
export { Badge, badgeVariants, type BadgeProps } from "./atoms/badge";
export { Button, buttonVariants, type ButtonProps } from "./atoms/button";
export { Label } from "./atoms/label";
export {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from "./molecules/command";
export { Input } from "./molecules/input";
export { Alert, alertVariants, type AlertProps } from "./molecules/alert";
export { Popover, PopoverAnchor, PopoverContent, PopoverTrigger } from "./molecules/popover";
export {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from "./molecules/select";
export { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "./molecules/tooltip";
export {
  Card,
  CardHeader,
  CardFooter,
  CardTitle,
  CardDescription,
  CardContent,
} from "./organisms/card";
