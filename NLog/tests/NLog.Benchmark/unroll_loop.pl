my $in = shift;
my $out = shift;
my $count = shift;

@lines = ();
open(IN, "$in") || die;
open(OUT, ">$out") || die;

while (<IN>)
{
    s/UNROLL_COUNT = 1/UNROLL_COUNT = $count/;

    if (m/BEGIN_UNROLL/)
    {
        $gathering = 1;
        @lines = ();
        next;
    }

    if (m/END_UNROLL/)
    {
        $gathering = 0;

        for ($i = 0; $i < $count; $i++)
        {
            printf(OUT "// loop #%d/%d\n", $i + 1, $count);
            for $l (@lines)
            {
                print OUT $l;
            }
        }
        next;
    }

    if ($gathering)
    {
        push(@lines,$_);
    }
    else
    {
        print OUT;
    }
}
close(OUT);
close(IN);
