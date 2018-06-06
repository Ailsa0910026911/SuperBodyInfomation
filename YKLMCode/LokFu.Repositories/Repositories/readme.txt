<Property Type="Byte" Name="IsDel" Nullable="false" />
<Property Name="IsDel" Type="Byte" Nullable="false" />

<ScalarProperty Name="IsDel" ColumnName="IsDel" />

<Condition ColumnName="IsDel" Value="0" />

ReportPropertyChanging("Amount");

ReportPropertyChanging("Frozen");