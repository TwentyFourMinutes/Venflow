---
uid: Guides.Advanced.Nullables
title: Nullables with Venflow
---

# Nullables with Venflow

Since C#8 null-able reference types are supported which help you writing better code and improves the IntelliSense. However for Venflow this also is a big deal. If you have specified `<Nullable>enable</Nullable>` in your `.csproj` file you opt-in for this feature. You will then be forced to mark all fields as null-able which are also marked as null-able in your database. That way Venflow can produce way more optimized code.

> [!WARNING] 
> You will also be forced to apply the null-able identifiers for all foreign-keys and navigation properties, if they are indeed null-able.
